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
using System.Collections.Generic;

namespace Jolt.Net
{

    /**
     * OpMode differentiates different flavors of Templatr
     *
     * Templatr can fill in leaf values as required in spec from a specified context, self or a hardcoded
     * default value. However whether or not that 'write' operation should go through, is determined by
     * this enum.
     *
     * All of these opModes validates if the if the source (map or list) and the key/index are valid,
     * i.e. not null or >= 0, etc.
     *
     * OVERWRITR always writes
     * DEFAULTR only writes when the the value at the key/index is null
     * DEFINR only writes when source does not contain the key/index
     *
     */
    public class OpMode
    {
        /**
         * Given a source map and a input key returns true if it is ok to go ahead with
         * write operation given a specific opMode
         */
        public virtual bool IsApplicable(JObject source, string key)
        {
            return source != null && key != null;
        }

        /**
         * Given a source list and a input index and original size of the list (when passed in as input)
         * returns true if it is ok to go ahead with write operation given a specific opMode
         */
        public virtual bool IsApplicable(JArray source, int reqIndex, int origSize)
        {
            return source != null && reqIndex >= 0 && origSize >= 0;
        }

        /**
         * Identifier OP prefix that is defined in SPEC
         */
        private readonly string _op;
        private readonly string _name;

        public OpMode(string op, string name)
        {
            _op = op;
            _name = name;
        }

        public string GetName() => _name;
        public string GetOp() => _op;

        public override string ToString() => _op + "modify";

        public static OpMode OVERWRITR = new OVERWRITR();
        public static OpMode DEFAULTR = new DEFAULTR();
        public static OpMode DEFINER = new DEFINER();

        /**
              * Static validity checker and instance getter from given op string
              */
        private static Dictionary<string, OpMode> _opModeMap = new Dictionary<string, OpMode>
        {
            { OVERWRITR.GetOp(), OVERWRITR },
            { DEFAULTR.GetOp(), DEFAULTR },
            { DEFINER.GetOp(), DEFINER }
        };

        public static bool IsValid(string op)
        {
            return _opModeMap.ContainsKey(op);
        }

        public static OpMode From(string op)
        {
            if (_opModeMap.TryGetValue(op, out var opMode))
            {
                return opMode;
            }
            throw new SpecException("OpMode " + op + " is not valid");
        }
    }

    class OVERWRITR : OpMode
    {
        public OVERWRITR() :
            base("+", "OVERWRITR")
        {
        }
    }
    class DEFAULTR : OpMode
    {
        public DEFAULTR() : 
            base("~", "DEFAULTR") 
        {
        }

        public override bool IsApplicable(JObject source, string key)
        {
            return base.IsApplicable(source, key) &&
                (!source.TryGetValue(key, out var value) || value == null);
        }

        public override bool IsApplicable(JArray source, int reqIndex, int origSize)
        {
            return base.IsApplicable(source, reqIndex, origSize) && source[reqIndex] == null;
        }
    }

    public class DEFINER : OpMode
    {
        public DEFINER() :
            base("_", "DEFINER")
        {
        }

        public override bool IsApplicable(JObject source, string key)
        {
            return base.IsApplicable(source, key) && !source.ContainsKey(key);
        }

        public override bool IsApplicable(JArray source, int reqIndex, int origSize)
        {
            return base.IsApplicable(source, reqIndex, origSize) &&
                    // only new index contains null
                    reqIndex >= origSize && source[reqIndex] == null;
        }
    };
}
