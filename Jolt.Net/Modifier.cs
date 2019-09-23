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

using System.Collections.Generic;

namespace Jolt.Net
{
#if FALSE
    /**
     * Base Templatr transform that to behave differently based on provided opMode
     */
    public abstract class Modifier : SpecDriven, IContextualTransform {

        private static readonly Dictionary<string, IFunction> STOCK_FUNCTIONS = new Dictionary<string, Function>
    {
        { "toLower", new Strings.toLowerCase() },
        { "toUpper", new Strings.toUpperCase() },
        { "concat", new Strings.concat() },
        { "join", new Strings.join() },
        { "split", new Strings.split() },
        { "substring", new Strings.substring() },
        { "trim", new Strings.trim() },
        { "leftPad", new Strings.leftPad() },
        { "rightPad", new Strings.rightPad() },

        { "min", new Math.min() },
        { "max", new Math.max() },
        { "abs", new Math.abs() },
        { "avg", new Math.avg() },
        { "intSum", new Math.intSum() },
        { "doubleSum", new Math.doubleSum() },
        { "longSum", new Math.longSum() },
        { "intSubtract", new Math.intSubtract() },
        { "doubleSubtract", new Math.doubleSubtract() },
        { "longSubtract", new Math.longSubtract() },
        { "divide", new Math.divide() },
        { "divideAndRound", new Math.divideAndRound() },

        { "toInteger", new Objects.toInteger() },
        { "toDouble", new Objects.toDouble() },
        { "toLong", new Objects.toLong() },
        { "toBoolean", new Objects.toBoolean() },
        { "toString", new Objects.toString() },
        { "size", new Objects.size() },

        { "squashNulls", new Objects.squashNulls() },
        { "recursivelySquashNulls", new Objects.recursivelySquashNulls() },

        { "noop", Function.noop },
        { "isPresent", Function.isPresent },
        { "notNull", Function.notNull },
        { "isNull", Function.isNull },

        { "firstElement", new Lists.firstElement() },
        { "lastElement", new Lists.lastElement() },
        { "elementAt", new Lists.elementAt() },
        { "toList", new Lists.toList() },
        { "sort", new Lists.sort() }
    };

    private readonly ModifierCompositeSpec _rootSpec;

    @SuppressWarnings( "unchecked" )
    private Modifier( object spec, OpMode opMode, Map<string, Function> functionsMap ) {
        if ( spec == null ){
            throw new SpecException( opMode.name() + " expected a spec of Map type, got 'null'." );
        }
        if ( ! ( spec instanceof Map ) ) {
            throw new SpecException( opMode.name() + " expected a spec of Map type, got " + spec.getClass().getSimpleName() );
        }

        if(functionsMap == null || functionsMap.isEmpty()) {
            throw new SpecException( opMode.name() + " expected a populated functions' map type, got " + (functionsMap == null?"null":"empty") );
        }

        functionsMap = Collections.unmodifiableMap( functionsMap );
        TemplatrSpecBuilder templatrSpecBuilder = new TemplatrSpecBuilder( opMode, functionsMap );
        rootSpec = new ModifierCompositeSpec( ROOT_KEY, (Map<string, object>) spec, opMode, templatrSpecBuilder );
    }

    @Override
    public object transform( final object input, final Map<string, object> context ) {

        Map<string, object> contextWrapper = new HashMap<>(  );
        contextWrapper.put( ROOT_KEY, context );

        MatchedElement rootLpe = new MatchedElement( ROOT_KEY );
        WalkedPath walkedPath = new WalkedPath();
        walkedPath.add( input, rootLpe );

        rootSpec.apply( ROOT_KEY, Optional.of( input), walkedPath, null, contextWrapper );
        return input;
    }

    /**
     * This variant of modifier creates the key/index is missing,
     * and overwrites the value if present
     */
    public static final class Overwritr extends Modifier {

        public Overwritr( object spec ) {
            this( spec, STOCK_FUNCTIONS );
        }

        public Overwritr( object spec, Map<string, Function> functionsMap ) {
            super( spec, OpMode.OVERWRITR, functionsMap );
        }
    }

    /**
     * This variant of modifier only writes when the key/index is missing
     */
    public static final class Definr extends Modifier {

        public Definr( final object spec ) {
            this( spec, STOCK_FUNCTIONS );
        }

        public Definr( object spec, Map<string, Function> functionsMap ) {
            super( spec, OpMode.DEFINER, functionsMap );
        }
    }

    /**
     * This variant of modifier only writes when the key/index is missing or the value is null
     */
    public static class Defaultr extends Modifier {

        public Defaultr( final object spec ) {
            this( spec, STOCK_FUNCTIONS );
        }

        public Defaultr( object spec, Map<string, Function> functionsMap ) {
            super( spec, OpMode.DEFAULTR, functionsMap );
        }
    }
#endif
}
