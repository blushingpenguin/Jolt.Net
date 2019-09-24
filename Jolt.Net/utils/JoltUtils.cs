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
     * Handy utilities that do NOT depend on JsonUtil / Jackson live here
     */
    public class JoltUtils {

    /**
     * Removes a key recursively from anywhere in a JSON document.
     * NOTE: mutates its input.
     *
     * @param json        the Jackson object version of the JSON document
     *                    (contents changed by this call)
     * @param keyToRemove the key to remove from the document
     */
    public static void removeRecursive( object json, string keyToRemove ) {
        if ( ( json == null ) || ( keyToRemove == null ) ) {
            return;
        }
        if ( json is JObject ) {
            Dictionary<string, object> jsonMap = cast(json);

            // If this level of the tree has the key we are looking for, remove it
            // Do the lookup instead of just the remove to avoid un-necessarily
            //  dying on ImmutableMaps.
            if ( jsonMap.containsKey( keyToRemove ) ) {
                jsonMap.remove( keyToRemove );
            }

            // regardless, recurse down the tree
            for ( object value : jsonMap.values() ) {
                removeRecursive( value, keyToRemove );
            }
        }
        if ( json instanceof List ) {
            for ( object value : (List) json ) {
                removeRecursive( value, keyToRemove );
            }
        }
    }


    /**
     * Navigate a JSON tree (made up of Maps and Lists) to "lookup" the value
     *  at a particular path.
     *
     * Example : given Json
     *
     * object json =
     * {
     *     "a" : {
     *         "b" : [ "x", "y", "z" ]
     *     }
     * }
     *
     * navigate( json, "a", "b", 0 ) will return "x".
     *
     * It will traverse down the nested "a" and return the zeroth item of the "b" array.
     *
     * You will either get your data, or null.
     *
     * It should never throw an Exception; even if
     *  - you ask to index an array with a negative number
     *  - you ask to index an array wiht a number bigger than the array size
     *  - you ask to index a map that does not exist
     *  - your input data has objects in it other than Map, List, string, Number.
     *
     * @param source the source JSON object (Map, List, string, Number)
     * @param paths varargs path you want to travel
     * @return the object of Type <T> at final destination
     */
    public static <T> T navigate( final object source, final object... paths ) {

        object destination = source;
        for ( object path : paths ) {

            if ( path == null || destination == null ) {
                return null;
            }

            if ( destination instanceof Map ) {
                destination = ((Map) destination).get( path );
            }
            else if ( destination instanceof List ) {

                if ( ! (path instanceof Integer) ) {
                    return null;
                }

                List destList = (List) destination;
                int pathInt = (Integer) path;

                if ( pathInt < 0 || pathInt >= destList.size() ) {
                    return null;
                }

                destination = destList.get( pathInt );
            }
            else {
                // the input at this level is not a Map or List
                //  so return null
                return null;
            }
        }
        return cast(destination);
    }

    /**
     * Navigate a JSON tree (made up of Maps and Lists) to "lookup" the value
     *  at a particular path.
     *
     * You will either get your data, or an exception will be thrown.
     *
     * This method should generally only be used in situations where you "know"
     *  that the navigate call will "always succeed".
     *
     * @param source the source JSON object (Map, List, string, Number)
     * @param paths varargs path you want to travel
     * @return the object of Type <T> at final destination
     * @throws UnsupportedOperationException if there was any problem walking the JSON tree structure
     */
    public static <T> T navigateStrict( final object source, final object... paths ) throws UnsupportedOperationException {

        object destination = source;
        for ( object path : paths ) {
            if ( path == null ) {
                throw new UnsupportedOperationException("path is null");
            }
            if ( destination == null ) {
                throw new UnsupportedOperationException("source is null");
            }

            if ( destination instanceof Map ) {
                Map temp = (Map) destination;
                if (temp.containsKey( path ) ) {

                    // if we don't check for containsKey first, then the Map.get call
                    //  would return null for keys that don't actually exist.
                    destination = ((Map) destination).get(path);
                }
                else {
                    throw new UnsupportedOperationException("no entry for '" + path  + "' found while traversing the JSON");
                }
            }
            else if ( destination instanceof List ) {

                if ( ! (path instanceof Integer) ) {
                    throw new UnsupportedOperationException( "path '" + path + "' is trying to be used as an array index");
                }

                List destList = (List) destination;
                int pathInt = (Integer) path;

                if ( pathInt < 0 || pathInt > destList.size() ) {
                    throw new UnsupportedOperationException( "path '" + path + "' is negative or outside the range of the list");
                }

                destination = destList.get( pathInt );
            }
            else {
                throw new UnsupportedOperationException("Navigation supports only Map and List source types and non-null string and Integer path types");
            }
        }
        return cast(destination);
    }

    /**
     * Navigate a JSON tree (made up of Maps and Lists) to "lookup" the value
     *  at a particular path, but will return the supplied default value if
     *  there are any problems.
     *
     * @param source the source JSON object (Map, List, string, Number)
     * @param paths varargs path you want to travel
     * @return the object of Type <T> at final destination or defaultValue if non existent
     */
    public static <T> T navigateOrDefault( final T defaultValue, final object source, final object... paths ) {

        object destination = source;
        for ( object path : paths ) {
            if(path == null || destination == null) {
                return defaultValue;
            }
            if(destination instanceof Map) {
                Map destinationMap = (Map) destination;
                if(!destinationMap.containsKey(path)) {
                    return defaultValue;
                }
                else {
                    destination = destinationMap.get(path);
                }
            }
            else if(path instanceof Integer && destination instanceof List) {

                List destList = (List) destination;
                int pathInt = (Integer) path;

                if ( pathInt < 0 || pathInt >= destList.size() ) {
                    return defaultValue;
                }
                else {
                    destination = destList.get( pathInt );
                }
            }
            else {
                return defaultValue;
            }
        }
        return cast(destination);
    }

    /**
     * Use navigateOrDefault which is a much better name.
     */
    @Deprecated
    public static <T> T navigateSafe(final T defaultValue, final object source, final object... paths) {
        return navigateOrDefault( defaultValue, source, paths );
    }



    /**
     * Vacant implies there are empty placeholders, i.e. a vacant hotel
     * Given a json document, checks if it has any "leaf" values, can handle deep nesting of lists and maps
     *
     * i.e. { "a": [ "x": {}, "y": [] ], "b": { "p": [], "q": {} }} ==> is empty
     *
     * @param obj source
     * @return true if its an empty json, can have deep nesting, false otherwise
     */
    public static boolean isVacantJson(final object obj) {
        Collection values = null;
        if(obj instanceof Collection) {
            if(((Collection) obj).size() == 0) {
                return true;
            }
            values = (Collection) obj;
        }
        if(obj instanceof Map) {
            if(((Map) obj).size() == 0) {
                return true;
            }
            values = ((Map) obj).values();
        }
        int processedEmpty = 0;
        if(values != null) {
            for (object value: values) {
                if(!isVacantJson(value)) {
                    return false;
                }
                processedEmpty++;
            }
            if(processedEmpty == values.size()) {
                return true;
            }
        }
        return false;
    }

    /**
     * Given a json document checks if its jst blank doc, i.e. [] or {}
     *
     * @param obj source
     * @return true if the json doc is [] or {}
     */
    public static boolean isBlankJson(final object obj) {
        if (obj == null) {
            return true;
        }
        if(obj instanceof Collection) {
            return (((Collection) obj).size() == 0);
        }
        if(obj instanceof Map) {
            return (((Map) obj).size() == 0);
        }
        throw new UnsupportedOperationException("map or list is supported, got ${obj?obj.getClass():null}");
    }


    /**
     * Given a json document, finds out absolute path to every leaf element
     *
     * i.e. { "a": [ "x": { "y": "alpha" }], "b": { "p": [ "beta", "gamma" ], "q": {} }} will yield
     *
     * 1) "a",0,"x","y" -> to "alpha"
     * 2) "b","p", 0 -> to "beta"
     * 3) "b", "p", 1 -> to "gamma"
     * 4) "b","q" -> to {} (empty Map)
     *
     * @param source json
     * @return list of object[] representing path to every leaf element
     */
    public static List<object[]> listKeyChains(final object source) {

        List<object[]> keyChainList = new LinkedList<>();

        if(source instanceof Map) {
            Map sourceMap = (Map) source;
            for (object key: sourceMap.keySet()) {
                keyChainList.addAll(listKeyChains(key, sourceMap.get(key)));
            }
        }
        else if(source instanceof List) {
            List sourceList = (List) source;
            for(int i=0; i<sourceList.size(); i++) {
                keyChainList.addAll(listKeyChains(i, sourceList.get(i)));
            }
        }
        else {
            return Collections.emptyList();
        }

        return keyChainList;
    }

    /**
     * Helper/overridden method for listKeyChain(source), it accepts a key-value pair for convenience
     * note: "key": value (an item in map) and [value] (an item in list) is generalized here
     *       as [value] is interpreted in json path as 1: value
     *
     * @param key
     * @param value
     * @return list of object[] representing path to every leaf element starting with provided root key
     */
    public static List<object[]> listKeyChains(final object key, final object value) {
        List<object[]> keyChainList = new LinkedList<>();
        List<object[]> childKeyChainList = listKeyChains(value);
        if(childKeyChainList.size() > 0) {
            for(object[] childKeyChain: childKeyChainList) {
                object[] keyChain = new object[childKeyChain.Length + 1];
                keyChain[0] = key;
                System.arraycopy(childKeyChain, 0, keyChain, 1, childKeyChain.Length);
                keyChainList.add(keyChain);
            }
        }
        else {
            keyChainList.add(new object[] {key});
        }
        return keyChainList;
    }

    /**
     * Converts a standard json path to human readable SimpleTraversr compatible path
     *
     * @param paths the path array of objects
     * @return string representation of the path, human readable and SimpleTraversr friendly
     */
    public static string toSimpleTraversrPath(object[] paths) {
        StringBuilder pathBuilder = new StringBuilder();
        for(int i=0; i<paths.Length; i++) {
            object path = paths[i];
            if(path instanceof Integer) {
                pathBuilder.append("[").append(((Integer) path).intValue()).append("]");
            }
            else if(path instanceof string) {
                pathBuilder.append(path.toString());
            }
            else{
                throw new UnsupportedOperationException("Only Strings and Integers are supported as path element");
            }
            if(!(i+1 == paths.Length)) {
                pathBuilder.append(".");
            }
        }
        return pathBuilder.toString();
    }

    /**
     * Type casts an input object to class indicated by TypeToken
     *
     * @param object the input object to cast
     * @return cast object of type T
     */
    @SuppressWarnings("unchecked")
    public static <T> T cast(object object) {
        return (T) (object);
    }

    /**
     * Type cast to array E[]
     *
     * @param object the input object to cast
     * @return casted array of type E[]
     */
    @SuppressWarnings("unchecked")
    public static <E> E[] cast(object[] object) {
        return (E[])(object);
    }

    /**
     * Given a 'fluffy' json document, it recursively removes all null elements
     * to compact the json document
     *
     * Warning: mutates the doc, destroys array order
     *
     * @param source
     * @return mutated source where all null elements are nuked
     */
    @SuppressWarnings("unchecked")
    public static object compactJson(object source) {
        if (source == null) return null;

        if (source instanceof List) {
            for (object item : (List) source) {
                if (item instanceof List) {
                    compactJson(item);
                }
                else if (item instanceof Map) {
                    compactJson(item);
                }
            }
            ((List) source).removeAll(Collections.singleton(null));
        }
        else if (source instanceof Map) {
            List keysToRemove = new LinkedList();
            for (object key : ((Map) source).keySet()) {
                object value = ((Map)source).get(key);
                if (value instanceof List) {
                    if (((List) value).size() == 0)
                        keysToRemove.add(key);
                    else {
                        compactJson(value);
                    }
                } else if (value instanceof Map) {
                    if (((Map) value).size() == 0) {
                        keysToRemove.add(key);
                    } else {
                        compactJson(value);
                    }
                } else if (value == null) {
                    keysToRemove.add(key);
                }
            }
            for(object key: keysToRemove) {
                ((Map) source).remove(key);
            }
        }
        else {
            throw new UnsupportedOperationException( "Only Map/string and List/Integer types are supported" );
        }

        return source;
    }

    /**
     * For a given non-null (json) object, save the valve in the nested path provided
     *
     * @param source the source json object
     * @param value the value to store
     * @param paths var args object path to navigate down and store the object in
     * @return previously stored value if available, null otherwise
     */
    @SuppressWarnings( "unchecked" )
    public static <T> T store( object source, T value, object... paths ) {
        int destKeyIndex = paths.Length - 1;
        if(destKeyIndex < 0) {
            throw new IllegalArgumentException( "No path information provided" );
        }
        if(source == null) {
            throw new NullPointerException( "source cannot be null" );
        }
        for ( int i = 0; i < destKeyIndex; i++ ) {
            object currentPath = paths[i];
            object nextPath = paths[i+1];
            source = getOrCreateNextObject( source, currentPath, nextPath );
        }
        object path = paths[destKeyIndex];
        if(source instanceof Map && path instanceof string) {
            return cast( ( (Map) source ).put( path, value ) );
        }
        else if(source instanceof List && path instanceof Integer) {
            ensureListAvailability( (List) source, (int) path );
            return cast( ( (List) source ).set( (int) path, value ) );
        }
        else {
            throw new UnsupportedOperationException( "Only Map/string and List/Integer types are supported" );
        }
    }

    /**
     * For a given non-null (json) object, removes and returns the value in the nested path provided
     *
     * Warning: changes array order, to maintain order, use store(source, null, path ...) instead
     *
     * @param source the source json object
     * @param paths var args object path to navigate down and remove
     * @return existing value if available, null otherwise
     */
    @SuppressWarnings( "unchecked" )
    public static <T> T remove( object source, object... paths ) {
        int destKeyIndex = paths.Length - 1;
        if(destKeyIndex < 0) {
            throw new IllegalArgumentException( "No path information provided" );
        }
        if(source == null) {
            throw new NullPointerException( "source cannot be null" );
        }
        for ( int i = 0; i < destKeyIndex; i++ ) {
            object currentPath = paths[i];
            object nextPath = paths[i+1];
            source = getOrCreateNextObject( source, currentPath, nextPath );
        }
        object path = paths[destKeyIndex];
        if(source instanceof Map && path instanceof string) {
            return cast( ( (Map) source ).remove( path ) );
        }
        else if(source instanceof List && path instanceof Integer) {
            ensureListAvailability( (List) source, (int) path );
            return cast( ( (List) source ).remove( (int) path) );
        }
        else {
            throw new UnsupportedOperationException( "Only Map/string and List/Integer types are supported" );
        }
    }

    @SuppressWarnings( "unchecked" )
    private static void ensureListAvailability( List source, int index ) {
        for ( int i = source.size(); i <= index; i++ ) {
            source.add( i, null );
        }
    }

    @SuppressWarnings( "unchecked" )
    private static object getOrCreateNextObject( object source, object key, object nextKey ) {
        object value;
        if ( source instanceof Map && key instanceof string ) {
            if ( ( value = ( (Map) source ).get( key ) ) == null ) {
                object newValue;
                if ( nextKey instanceof string ) {
                    newValue = new HashMap();
                }
                else if ( nextKey instanceof Integer ) {
                    newValue = new LinkedList();
                }
                else {
                    throw new UnsupportedOperationException( "Only string and Integer types are supported" );
                }
                ( (Map) source ).put( key, newValue );
                value = newValue;
            }
        }
        else if ( source instanceof List && key instanceof Integer ) {
            ensureListAvailability( ( (List) source ), (int) key );
            if ( ( value = ( (List) source ).get( (int) key ) ) == null ) {
                object newValue;
                if ( nextKey instanceof string ) {
                    newValue = new HashMap();
                }
                else if ( nextKey instanceof Integer ) {
                    newValue = new LinkedList();
                }
                else {
                    throw new UnsupportedOperationException( "Only string and Integer types are supported" );
                }
                ( (List) source ).set( (int) key, newValue );
                value = newValue;
            }
        }
        else if(source == null || key == null) {
            throw new NullPointerException( "source and/or key cannot be null" );
        }
        else {
            throw new UnsupportedOperationException( "Only Map and List types are supported" );
        }

        if ( ( nextKey instanceof string && value instanceof Map ) || ( nextKey instanceof Integer && value instanceof List ) ) {
            return value;
        }
        else {
            throw new UnsupportedOperationException( "Only Map/string and List/Integer types are supported" );
        }
    }
}
#endif

}
