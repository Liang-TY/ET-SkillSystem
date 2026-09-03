// Copyright 2025 Code Philosophy
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

using Luban.Defs;

namespace Luban.RawDefs;

public class RawTable
{
    public  string Package { get; set; }
    private string m_Namespace;

    public string Namespace
    {
        get => m_Namespace;
        set => (m_Namespace, Package) = ETNamespacePackage.Get(value);
    }

    public string Name { get; set; }

    public string Index { get; set; }

    private string m_ValueType;

    public string ValueType
    {
        get => m_ValueType;
        set
        {
            (m_ValueType, var package) = ETNamespacePackage.Get(value);
            if (!string.IsNullOrEmpty(package))
            {
                Package = package;
            }
        }
    }

    public bool ReadSchemaFromFile { get; set; }

    public TableMode Mode { get; set; }

    public string Comment { get; set; }

    public Dictionary<string, string> Tags { get; set; }

    public List<string> Groups { get; set; } = new();

    public List<string> InputFiles { get; set; } = new();

    public string OutputFile { get; set; }
}
