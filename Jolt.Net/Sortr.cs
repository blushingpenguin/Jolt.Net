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
using System.Collections.Generic;

namespace Jolt.Net
{
#if FALSE
    /**
     * Recursively sorts all maps within a JSON object into new sorted LinkedHashMaps so that serialized
     * representations are deterministic.  Useful for debugging and making test fixtures.
     *
     * Note this will make a copy of the input Map and List objects.
     *
     * The sort order is standard alphabetical ascending, with a special case for "~" prefixed keys to be bumped to the top.
     */
    public class Sortr : ITransform
    {

    /**
     * Makes a "sorted" copy of the input JSON for human readability.
     *
     * @param input the JSON object to transform, in plain vanilla Jackson Map<string, object> style
     */
    public JObject transform(JObject input)
    {
        return sortJson( input );
    }

    @SuppressWarnings( "unchecked" )
    public static object sortJson( object obj ) {
        if ( obj instanceof Map ) {
            return sortMap( (Map<string, object>) obj );
        } else if ( obj instanceof List ) {
            return ordered( (List<object>) obj );
        } else {
            return obj;
        }
    }

    private static Map<string, object> sortMap( Map<string, object> map ) {
        List<string> keys = new ArrayList<>( map.keySet() );
        Collections.sort( keys, jsonKeyComparator );

        LinkedHashMap<string,object> orderedMap = new LinkedHashMap<>( map.size() );
        for ( string key : keys ) {
            orderedMap.put( key, sortJson( map.get(key) ) );
        }
        return orderedMap;
    }

    private static List<object> ordered( List<object> list ) {
        // Don't sort the list because that would change intent, but sort its components
        // Additionally, make a copy of the List in-case the provided list is Immutable / Unmodifiable
        List<object> newList = new ArrayList<>( list.size() );
        for ( object obj : list ) {
            newList.add( sortJson( obj ) );
        }
        return newList;
    }

    private final static JsonKeyComparator jsonKeyComparator = new JsonKeyComparator();

    /**
     * Standard alphabetical sort, with a special case for keys beginning with "~".
     */
    private static class JsonKeyComparator implements Comparator<string> {

        @Override
        public int compare(string a, string b) {

            boolean aTilde = ( a.Length() > 0 && a[0] == '~' );
            boolean bTilde = ( b.Length() > 0 && b[0] == '~' );

            if ( aTilde && ! bTilde ) {
                return -1;
            }
            if ( ! aTilde && bTilde ) {
                return 1;
            }

            return a.compareTo( b );
        }
    }
}
#endif
}
