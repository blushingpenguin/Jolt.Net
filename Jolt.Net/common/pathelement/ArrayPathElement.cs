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
using System;

namespace Jolt.Net
{
    public class ArrayPathElement : BasePathElement, IMatchablePathElement, IEvaluatablePathElement
    {
        public enum ArrayPathType { AUTO_EXPAND, REFERENCE, HASH, TRANSPOSE, EXPLICIT_INDEX }

        private readonly ArrayPathType _arrayPathType;
        private readonly IPathReference _ref;
        private readonly TransposePathElement _transposePathElement;

        private readonly string _canonicalForm;
        private readonly string _arrayIndex;

        public ArrayPathElement(string key) :
            base(key)
        {
            if (key[0] != '[' || key[key.Length - 1] != ']')
            {
                throw new SpecException("Invalid ArrayPathElement key:" + key);
            }

            ArrayPathType apt;
            IPathReference r = null;
            TransposePathElement tpe = null;
            string aI = "";

            if (key.Length == 2)
            {
                apt = ArrayPathType.AUTO_EXPAND;
                _canonicalForm = "[]";
            }
            else
            {
                string meat = key.Substring(1, key.Length - 2);
                char firstChar = meat[0];

                if (AmpReference.TOKEN == firstChar)
                {
                    r = new AmpReference(meat);
                    apt = ArrayPathType.REFERENCE;
                    _canonicalForm = "[" + r.GetCanonicalForm() + "]";
                }
                else if (HashReference.TOKEN == firstChar)
                {
                    r = new HashReference(meat);
                    apt = ArrayPathType.HASH;

                    _canonicalForm = "[" + r.GetCanonicalForm() + "]";
                }
                else if ('@' == firstChar)
                {
                    apt = ArrayPathType.TRANSPOSE;

                    tpe = TransposePathElement.Parse(meat);
                    _canonicalForm = "[" + tpe.GetCanonicalForm() + "]";
                }
                else
                {
                    aI = VerifyStringIsNonNegativeInteger(meat);
                    if (aI != null) {
                        apt = ArrayPathType.EXPLICIT_INDEX;
                        _canonicalForm = "[" + aI + "]";
                    }
                    else
                    {
                        throw new SpecException("Bad explict array index:" + meat + " from key:" + key);
                    }
                }
            }

            _transposePathElement = tpe;
            _arrayPathType = apt;
            _ref = r;
            _arrayIndex = aI;
        }

        public override string GetCanonicalForm() => _canonicalForm;

        public string Evaluate(WalkedPath walkedPath)
        {
            switch (_arrayPathType) {
                case ArrayPathType.AUTO_EXPAND:
                    return _canonicalForm;

                case ArrayPathType.EXPLICIT_INDEX:
                    return _arrayIndex;

                case ArrayPathType.HASH:
                    MatchedElement element = walkedPath.ElementFromEnd(_ref.GetPathIndex()).MatchedElement;
                    return element.GetHashCount().ToString();

                case ArrayPathType.TRANSPOSE:
                    string key = _transposePathElement.Evaluate(walkedPath);
                    return VerifyStringIsNonNegativeInteger(key);

                case ArrayPathType.REFERENCE:
                    {
                        MatchedElement lpe = walkedPath.ElementFromEnd(_ref.GetPathIndex()).MatchedElement;
                        string keyPart;

                        if (_ref is IPathAndGroupReference pagr)
                        {
                            keyPart = lpe.GetSubKeyRef(pagr.GetKeyGroup());
                        }
                        else
                        {
                            keyPart = lpe.GetSubKeyRef(0);
                        }

                        return VerifyStringIsNonNegativeInteger(keyPart);
                    }
                default:
                    throw new InvalidOperationException("ArrayPathType enum added two without updating this switch statement.");
            }
        }

        /**
         * @return the string version of a non-Negative integer, else null
         */
        private static string VerifyStringIsNonNegativeInteger(string key)
        {
            // Jolt should not throw any exceptions just because the input data does not match what is expected.
            // Thus the exception is being swallowed.
            return Int32.TryParse(key, out var number) ? key : null;
        }

        public int? GetExplicitArrayIndex()
        {
            if (!Int32.TryParse(_arrayIndex, out var num))
            {
                return null;
            }
            return num;
        }

        public bool IsExplicitArrayIndex()
        {
            return _arrayPathType == ArrayPathType.EXPLICIT_INDEX;
        }

        public MatchedElement Match(string dataKey, WalkedPath walkedPath)
        {
            string evaled = Evaluate(walkedPath);
            if (evaled == dataKey)
            {
                int? origSizeOptional = walkedPath.LastElement().OrigSize;
                if (origSizeOptional.HasValue)
                {
                    return new ArrayMatchedElement(evaled, origSizeOptional.Value);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
    }
}
