/*
 * Copyright 2013 Bazaarvoice, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jolt.Net
{
    public abstract class Key
    {
        /**
         * Factory-ish method that recursively processes a Map<string, object> into a Set<Key> objects.
         *
         * @param spec Simple Jackson default Map<string,object> input
         * @return Set of Keys from this level in the spec
         */
        public static HashSet<Key> ParseSpec(JObject spec )
        {
            return ProcessSpec( false, spec );
        }

        /**
         * Recursively walk the spec input tree.  Handle arrays by telling DefaultrKeys if they need to be ArrayKeys, and
         *  to find the max default array.Length.
         */
        private static HashSet<Key> ProcessSpec(bool parentIsArray, JObject spec)
        {
            // TODO switch to List<Key> and sort before returning
            var result = new HashSet<Key>();

            foreach (var kv in spec)
            {
                if (parentIsArray)
                {
                    result.Add(new ArrayKey(kv.Key, kv.Value)); // this will recursively call processSpec if needed
                }
                else
                {
                    result.Add(new MapKey(kv.Key, (JObject)kv.Value)); // this will recursively call processSpec if needed
                }
            }

            return result;
        }


        private static readonly string OR_INPUT_REGEX = "\\" + Defaultr.WildCards.OR;
        private static readonly KeyPrecedenceComparator _keyComparator = new KeyPrecedenceComparator();

        // Am I supposed to be parent of an array?  If so I need to make sure that I inform
        //  my children they need to be ArrayKeys, and I need to make sure that the output array
        //  I will write to is big enough.
        private bool _isArrayOutput = false;
        private OPS _op;
        private int _orCount = 0;
        private int _outputArraySize = -1;

        protected HashSet<Key> _children = null;
        protected object _literalValue = null;

        protected string _rawKey;
        protected List<string> _keyStrings;

        public Key(string rawJsonKey, JToken spec)
        {
            _rawKey = rawJsonKey;
            if (rawJsonKey.EndsWith(Defaultr.WildCards.ARRAY))
            {
                _isArrayOutput = true;
                _rawKey = _rawKey.Replace(Defaultr.WildCards.ARRAY, "");
            }

            _op = OPSUtils.Parse(_rawKey);

            switch (_op)
            {
                case OPS.OR:
                    _keyStrings = new List<string>(Regex.Split(_rawKey, Key.OR_INPUT_REGEX));
                    _orCount = _keyStrings.Count;
                    break;
                case OPS.LITERAL:
                    _keyStrings = new List<string>();
                    _keyStrings.Add(_rawKey);
                    break;
                case OPS.STAR:
                    _keyStrings = new List<string>();
                    break;
                default:
                    throw new InvalidOperationException( "Someone has added an op type without changing this method." );
            }

            // Spec is string -> Map   or   string -> Literal only
            if (spec.Type == JTokenType.Object)
            {
                var children = ProcessSpec(IsArrayOutput(), (JObject)spec);

                if (IsArrayOutput())
                {
                    // loop over children and find the max literal value
                    foreach (Key childKey in _children)
                    {
                        int childValue = childKey.GetLiteralIntKey();
                        if (childValue > _outputArraySize)
                        {
                            _outputArraySize = childValue;
                        }
                    }
                }
            }
            else
            {
                // literal such as string, number, or JSON array
                _literalValue = spec;
            }
        }

        /**
         * This is the main "recursive" method.   The defaultee should never be null, because
         *  the defaultee wasn't null, it was null and we created it, OR there was
         *  a mismatch between the Defaultr Spec and the input, and we didn't recurse.
         */
        public void ApplyChildren(object defaultee)
        {

            if ( defaultee == null ) {
                throw new TransformException( "Defaultee should never be null when " +
                        "passed to the applyChildren method." );
            }

            // This has nothing to do with this being an ArrayKey or MapKey, instead this is about
            //  this key being the parent of an Array in the output.
            if (IsArrayOutput() && defaultee is List<object> defaultList) {
                // Extend the defaultee list if needed
                for ( int index = defaultList.Count - 1; index < GetOutputArraySize(); index++ ) {
                    defaultList.Add( null );
                }
            }

            // Find and sort the children DefaultrKeys by precedence: literals, |, then *
            var sortedChildren = new List<Key>();
            sortedChildren.AddRange(_children);
            sortedChildren.Sort(_keyComparator);

            foreach (Key childKey in sortedChildren)
            {
                childKey.ApplyChild(defaultee);
            }
        }

        protected abstract int GetLiteralIntKey();

        /**
         * Apply this Key to the defaultee.
         *
         * If this Key is a WildCard key, this may apply to many entries in the container.
         */
        protected abstract void ApplyChild(object container);

        public int GetOrCount() => _orCount;

        public bool IsArrayOutput() => _isArrayOutput;

        public OPS GetOp() => _op;

        public int GetOutputArraySize() => _outputArraySize;

        public object CreateOutputContainerObject()
        {
            if ( IsArrayOutput() )
            {
                return new List<object>();
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }

        public class KeyPrecedenceComparator : IComparer<Key>
        {
            private readonly OpsPrecedenceComparator _opsComparator = new OpsPrecedenceComparator();

            public int Compare(Key a, Key b)
            {
                int opsEqual = _opsComparator.Compare(a.GetOp(), b.GetOp());

                if (opsEqual == 0 && OPS.OR == a.GetOp() && OPS.OR == b.GetOp())
                {
                    // For deterministic behavior, sub sort on the specificity of the OR and then alphabetically on the rawKey
                    //   For the Or, the more star like, the higher your value
                    //   If the or count matches, fall back to alphabetical on the rawKey from the spec file
                    return (a.GetOrCount() < b.GetOrCount() ? -1 : (
                        a.GetOrCount() == b.GetOrCount() ? a._rawKey.CompareTo( b._rawKey ) : 1 ) );
                }

                return opsEqual;
            }
        }
    }
}
