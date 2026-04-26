// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System.Text.RegularExpressions;

using JetBrains.Annotations;

namespace LughSharp.Source.Utils.Json;

/// <summary>
/// Efficient JSON parser that extracts values matching specified patterns using a single pass.
/// It conveniently collects values for processing in batches or at once. Values not collected
/// have minimal overhead, ideal for processing a subset of large JSON.
/// <br/>
/// 
/// <para>
/// Match<br/>
/// =====<br/>
/// 
/// Match objects, arrays, or field names (not field values) in the JSON:
/// <li><c>/</c> - Path separator.</li>
/// <li><c>> name</c> - Matches field "name".</li>
/// <li><c>> a,b,c</c> - Matches fields "a", "b", or "c".</li>
/// <li><c>> *</c> - Matches one object, array, or field.</li>
/// <li><c>> **</c> - Matches zero or more objects, arrays, or fields.</li>
/// </para>
///
/// <para>
/// Capture<br/>
/// =======<br/>
///
/// Surround any match with parenthesis to capture that value.
/// <li><c>(name)</c> - Captures field "name".</li>
/// <li><c>(a),b,(c,d)</c> - Captures multiple fields.</li>
/// <li><c>(*)</c> or <c>(**)</c> - Captures wildcard matches.</li>
/// </para>
///
/// <para>
/// Process<br/>
/// =======<br/>
///
/// Use <c>@</c>after a match to control when processors receive captured values.
/// <li><c>devices/(id,name)</c> - Without <c>@</c> values are processed once at the end and
/// only the first values matched are captured.</li>
/// <li><c>> devices/(id@,name@)</c> - Process "id" and "name" as soon as each is captured.</li>
/// <li><c>> devices/(id,name)@</c> - Same as <c>(id@,name@)</c>.</li>
/// <li><c>> devices@/(id,name)</c> - Process "id" and "name" as a pair for each device.</li>
/// <li><c>> *@/(id,name)</c> - Any match can be annotated to process.</li>
/// <li><c>> **@/(id,name)</c> - All captures after <c>**@</c> are processed as soon as each is
/// captured.</li>
/// </para>
///
/// <para>
/// Arrays<br/>
/// ======<br/>
///
/// Arrays are captured as-is:
/// <li><c>data/(items)</c> with <c>{data:{items:[1,2,3]}}</c> gives: <c>{items=[1,2,3]}</c>
/// Use <c>[]</c> in a capture to collect into an array rather than as a single value.</li>
/// <li><c>*&#47;(id)</c> with <c>{first:{id:1},second:{id:2}}</c> gives: <c>{id=1}</c>
/// (parsing ends after first match)</li>
/// <li><c>*&#47;(id[])</c> gives: <c>{id=[1, 2]}</c> (all matches collected in an array)</li>
/// </para>
///
/// <para>
/// Examples<br/>
/// ========<br/>
/// 
/// <c>{users:[{name:nate},{name:iva}]}</c><br/>
/// Process each user name: <c>users@/(name)</c>
/// <li>The JSON root is matched implicitly.</li>
/// <li><c>users</c> - matches the <c>users:[</c> array. The field name and value are matched together.</li>
/// <li><c>@</c> - processes captured values at the end of each array item.</li>
/// <li><c>(name)</c> - captures the "name" field and value.</li>
/// <li>Result: Processors are called with <c>nate</c> and again with <c>iva</c>.</li>
/// </para>
/// 
/// <para>
/// <c>{config:{port:8081}}</c><br/>
/// Get the first port from "config" found at any depth: <c>**&#47;config/(port)</c><br/>
/// Result: Processors are called with: <c>8081</c>
/// </para>
/// <para>
/// <c>{services:[{status:up},{status:down},{status:failed}]}</c><br/>
/// Get all service statuses in an array: <c>services/*&#47;(status[])</c><br/>
/// Result: Processors are called with: <c>[up, down, failed]</c>
/// </para>
/// <para>
/// <c>{items:[{id:123,type:cookies},{id:456,type:cake}]}</c><br/>
/// Process each "id" and "type" pair: <c>items@/(id,type)</c><br/>
/// Result: Since there are multiple captures, processors are called with an object: <c>{id=123,type=cookies}</c> and
/// <c>{id=456,type=cake}</c>
/// </para>
///
/// <para>
/// Escaping<br/>
/// ========<br/>
/// Use special characters <c>/,*@()[]',\</c> by surrounding them with single quotes.
/// <li><c>email/'moo@dog.com'</c> - Uses "moo@dog.com" as a literal string.</li>
/// <li><c>words/'can''t'</c> - Escape single quote with two single quotes.</li>
/// </para>
///
/// <para>
/// Keys<br/>
/// ====<br/>
/// Capture keys using <c>()</c>.
/// <li><c>object/()</c> with <c>{object:{a:1,b:2,c:3}}</c> gives: <c>a</c> (parsing ends
/// after first match)</li>
/// <li><c>object/()[]</c> gives: <c>[a,b,c]</c> (all keys collected in an array)</li>
/// </para>
///
/// <para>
/// Behavior notes<br/>
/// ==============<br/>
/// <li>When multiple patterns are added they match in parallel, each with independent
/// capture and processing.</li>
/// <li><c>reject()</c> - prevents further matching at this level or deeper, useful for
/// filtering. A pattern can reject a different pattern.</li>
/// <li><c>clear()</c> - discards unprocessed captured values.</li>
/// <li><c>parseValue()</c> - returns captures at the end of parsing for convenience rather
/// than using a processor.</li>
/// <li><c>parseStart()</c> - and <c>parseEnd()</c> can be overidden to take action before
/// or after parsing.</li>
/// <li><c>end()</c> - prevents further matching and ends parsing. <c>stop()</c> does the
/// same but also clears.</li>
/// <li>If not using <c>*@</c>, <c>**@</c>, or <c>[]</c> parsing ends once all specified
/// values are captured.</li>
/// <li>A single capture before processing provides the value directly, multiple captures
/// provide an object.</li>
/// <li>No patterns or a <c>""</c> pattern captures the entire JSON document.</li>
/// </para>
///
/// <para>
/// JsonValues<br/>
/// ==========<br/>
/// <li>JsonValue instances are reused for <c>@</c> processing, references should not be kept.</li>
/// <li>JsonValue references can safely be held only by processors called at the end of parsing.</li>
/// <li>JsonValue types are object, array, null, boolean, or String.</li>
/// <li>Numbers are provided as String, deferring number parsing until explicitly converted
/// (eg <see cref="JsonValue.AsFloat()"/>).</li>
/// </para>
///
/// <para>
/// Accessing values<br/>
/// ================<br/>
/// Use <c>process()</c> or a processor to handle captures one by one. Use <c>parseValue()</c> to
/// obtain captures directly:
/// 
/// <code>
/// JsonValue values = matcher.ParseValue(json);
/// string error = value.GetString("message", null);
/// if (error != null)
/// {
///     Fail(error);
/// }
/// return value.GetString("access_token");
/// </code>
/// </para>
/// </summary>
[PublicAPI]
public class JsonMatcher : JsonSkimmer
{
    private const int None    = 0;
    private const int Match   = 0b00000001;
    private const int Process = 0b00000010;
    private const int Capture = 0b00000100;
    private const int Array   = 0b00001000;
    private const int Keys    = 0b00010000;
    private const int Single  = 0b00100000;

    private JsonMatcher.IProcessor? _processor;
    private Regex[]?                _patterns;
    private Regex[]?                _original;
    private Regex[]?                _all;

    private int _total;
    private int _endCaptures;

    // ========================================================================
    
    private interface IProcessor
    {
        void Process( JsonValue value );
    }
}

// ============================================================================
// ============================================================================