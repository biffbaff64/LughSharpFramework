// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
public interface IFileProcessor
{
    List< Regex >                     InputRegex      { get; set; }
    List< FileProcessor.Entry >       OutputFilesList { get; set; }
    string                            OutputSuffix    { get; set; }
    bool                              Recursive       { get; set; }
    bool                              FlattenOutput   { get; set; }
    Comparison< FileInfo >            Comparator      { get; set; }
    Comparison< FileProcessor.Entry > EntryComparator { get; set; }
    bool                              CountOnly       { get; set; }
    List< DirectoryInfo >             DirsToIgnore    { get; set; }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    List< FileProcessor.Entry > Process( string inputFileOrDir, string? outputRoot );

    /// <summary>
    /// Processes the specified input file or directory, as specified by the provided
    /// <see cref="FileInfo"/> objects.
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile"/>. </returns>
    List< FileProcessor.Entry > Process( DirectoryInfo? inputFileOrDir, DirectoryInfo? outputRoot );

    /// <summary>
    /// Processes a collection of files for sending to the output folder.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    List< FileProcessor.Entry > Process( FileInfo[] files, DirectoryInfo? outputRoot );

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <param name="outputDir"></param>
    /// <param name="dirToEntries"></param>
    /// <param name="depth"></param>
    void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                  Dictionary< DirectoryInfo, List< FileProcessor.Entry >? > dirToEntries, int depth );

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <param name="outputDir"></param>
    /// <param name="stringToEntries"></param>
    /// <param name="depth"></param>
    void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                  Dictionary< string, List< FileProcessor.Entry > > stringToEntries, int depth );

    /// <summary>
    /// </summary>
    /// <param name="entry"></param>
    void ProcessFile( FileProcessor.Entry entry );

    /// <summary>
    /// </summary>
    /// <param name="entryDir"></param>
    /// <param name="files"></param>
    void ProcessDir( FileProcessor.Entry entryDir, List< FileProcessor.Entry > files );

    /// <summary>
    /// Adds a processed <see cref="FileProcessor.Entry"/> to the <see cref="OutputFilesList"/>.
    /// This method should be called by:-
    /// <li><see cref="ProcessFile"/> or,</li>
    /// <li><see cref="ProcessDir"/></li>
    /// if the return value of <see cref="System.Diagnostics.Process"/> or
    /// <see cref="Process(FileInfo[],DirectoryInfo)"/> should return all the processed
    /// files.
    /// </summary>
    /// <param name="entry"></param>
    void AddProcessedFile( FileProcessor.Entry entry );

    /// <summary>
    /// </summary>
    /// <param name="regexes"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    IFileProcessor AddInputRegex( params string[] regexes );

    /// <summary>
    /// Adds a case insensitive suffix for matching input files.
    /// </summary>
    /// <param name="suffixes"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    IFileProcessor AddInputSuffix( params string[] suffixes );
}