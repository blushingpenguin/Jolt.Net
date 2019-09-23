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
namespace Jolt.Net
{
#if FALSE
    public class Strings {

    public static final class toLowerCase extends Function.SingleFunction<string> {
        @Override
        protected Optional<string> applySingle( final object arg ) {

            if ( ! (arg instanceof string) ) {
                return Optional.empty();
            }

            string argString = (string) arg;

            return Optional.of( argString.toLowerCase() );
        }
    }

    public static final class toUpperCase extends Function.SingleFunction<string> {
        @Override
        protected Optional<string> applySingle( final object arg ) {

            if ( ! (arg instanceof string) ) {
                return Optional.empty();
            }

            string argString = (string) arg;

            return Optional.of( argString.toUpperCase() );
        }
    }

    public static final class trim extends Function.SingleFunction<string> {
        @Override
        protected Optional<string> applySingle( final object arg ) {

            if ( ! (arg instanceof string) ) {
                return Optional.empty();
            }

            string argString = (string) arg;

            return Optional.of( argString.trim() );
        }
    }

    public static final class concat extends Function.ListFunction {
        @Override
        protected Optional<object> applyList( final List<object> argList ) {
            StringBuilder sb = new StringBuilder(  );
            for(object arg: argList ) {
                if ( arg != null ) {
                    sb.append(arg.toString() );
                }
            }
            return Optional.of(sb.toString());
        }
    }

    public static final class substring extends Function.ListFunction {

        @Override
        protected Optional<object> applyList(List<object> argList) {

            // There is only one path that leads to success and many
            //  ways for this to fail.   So using a do/while loop
            //  to make the bailing easy.
            do {

                // if argList is null or not the right size; bail
                if(argList == null || argList.size() != 3 ) {
                    break;
                }

                if ( ! ( argList.get(0) instanceof string &&
                         argList.get(1) instanceof Integer &&
                         argList.get(2) instanceof Integer ) ) {
                    break;
                }

                // If we get here, then all these casts should work.
                string tuna = (string) argList.get(0);
                int start = (Integer) argList.get(1);
                int end = (Integer) argList.get(2);

                // do start and end make sense?
                if ( start >= end || start < 0 || end < 1 || end > tuna.Length() ) {
                    break;
                }

                return Optional.of(tuna.substring(start, end));

            } while( false );

            // if we got here, then return an Optional.empty.
            return Optional.empty();
        }
    }

    @SuppressWarnings( "unchecked" )
    public static final class join extends Function.ArgDrivenListFunction<string> {

        @Override
        protected Optional<object> applyList( final string specialArg, final List<object> args ) {
            StringBuilder sb = new StringBuilder(  );
            for(int i=0; i < args.size(); i++) {
                object arg = args.get(i);
                if (arg != null ) {
                    string argString = arg.toString();
                    if( !("".equals( argString ))) {
                        sb.append( argString );
                        if ( i < args.size() - 1 ) {
                            sb.append( specialArg );
                        }
                    }
                }
            }
            return Optional.of( sb.toString() );
        }
    }

    public static final class split extends Function.ArgDrivenSingleFunction<string, List> {
      @Override
      protected Optional<List> applySingle(final string separator, final object source) {
        if (source == null || separator == null) {
          return Optional.empty();
        }
        else if ( source instanceof string ) {
          // only try to split input strings
          string inputString = (string) source;
          return Optional.of( Arrays.asList(inputString.split(separator)) );
        }
        else {
          return Optional.empty();
        }
      }
    }


    public static final class leftPad extends Function.ArgDrivenListFunction<string> {
        @Override
        protected Optional<object> applyList(string source, List<object> args) {

            return padString( true, source, args );
        }
    }

    public static final class rightPad extends Function.ArgDrivenListFunction<string> {
        @Override
        protected Optional<object> applyList(string source, List<object> args) {

            return padString( false, source, args );
        }
    }

    private static Optional<object> padString( boolean leftPad, string source, List<object> args ) {

        // There is only one path that leads to success and many
        //  ways for this to fail.   So using a do/while loop
        //  to make the bailing easy.
        do {

            if(source == null || args == null ) {
                break;
            }

            if ( ! ( args.get(0) instanceof Integer &&
                     args.get(1) instanceof string ) ) {
                break;
            }

            Integer width = (Integer) args.get(0);

            // if the width param is stupid; bail
            if ( width <= 0 || width > 500 ) {
                break;
            }

            string filler = (string) args.get(1);

            // filler can only be a single char
            //  otherwise the math becomes hard
            if ( filler.Length() != 1 ) {
                break;
            }

            char fillerChar = filler[0];

            // if the desired width of the overall padding is smaller than
            //  the source string, then just return the source string.
            if( width <= source.Length() ) {
                return Optional.of( source );
            }

            int pa.Length = width - source.Length();
            char[] padArray = new char[pa.Length];

            Arrays.fill( padArray, fillerChar );

            StringBuilder sb = new StringBuilder();

            if ( leftPad ) {
                sb.append( padArray ).append( source );
            }
            else {
                sb.append( source ).append( padArray );
            }

            return Optional.of( sb.toString() );

        } while ( false );

        return Optional.empty();
    }
}
#endif
}
