# About XPathReader
XPathReader is a .NET/C# library that simplifies the process of reading and extracting data from XML documents using XPath expressions. It uses Source Generators to create a custom reader class based on provided XPath expressions, allowing for memory efficient and fast access to XML data. The name and idea were inspired by Microsoft's [XPathReader](https://learn.microsoft.com/en-us/previous-versions/dotnet/articles/ms950778(v=msdn.10)?redirectedfrom=MSDN) from early .Net days. 

# Features
- **Source Generators**: Automatically generates a reader class based on specified XPath expressions.
- **Memory Efficient**: Uses `XmlReader` for low memory consumption when processing large XML files.
- **Multiple XPath Support**: Reads the document once and extracts data for multiple XPath expressions in a single pass.

# Usage example

<details>
<summary>bookstore.xml</summary>

```xml
<?xml version="1.0" encoding="UTF-8"?>
<bookstore>
    <books>
       <book category="fiction">
           <title>GFG</title>
           <author>Fictional book </author>
           <year>2002</year>
           <price>5000</price>
       </book>
       <book category="non-fiction">
            <title>GFG 2 </title>
            <author>non fictional book </author>
            <year>2020</year>
            <price>1000</price>
       </book>
    </books>
    <journals>
        <journal>
            <title>Journal 1</title>
            <publisher>Publisher 1</publisher>
        </journal>
        <journal>
            <title>Journal 2</title>
            <publisher>Publisher 2</publisher>
        </journal>
    </journals>
</bookstore>
```
</details>

```csharp
using ARTX.XPath;

public partial class Program
{
    [GeneratedXPathReader("/bookstore/books/book/title|/bookstore/journals/journal/publisher")]
    public static partial XPathReader BookStoreReader { get; }

    static void Main(string[] args)
    {
        FileStream stream = File.OpenRead("bookstore.xml");

        // There is also an async implementation.
        // await foreach (ReadResult item in BookStoreReader.ReadAsync(stream, cancellationToken))
        foreach (ReadResult item in BookStoreReader.Read(stream))
        {
            Console.WriteLine(item.NodeReader.ReadOuterXml());
        }

        // Output:
        //<title>GFG</title>
        //<title>GFG 2</title >
        //<publisher>Publisher 1</publisher>
        //<publisher>Publisher 2</publisher>
    }
}
```

# Installation
You can install the XPathReader library via NuGet Package Manager:
```
dotenet add package ARTX.XPathReader
```

# Requirements
- .NET Framework 4.8 or later (requires Language Version 9.0 or later)
- Any .NET version that supports Source Generators

# Usage 
1. Add the `GeneratedXPathReader` attribute to a partial method or property, specifying the XPath expressions.
    - You can provide multiple XPath expressions separated by `|` or new line.
1. Call the generated `Read` or `ReadAsync` method, passing in a `Stream` containing the XML data.
1. These methods return an `IEnumerable<ReadResult>` or `IAsyncEnumerable<ReadResult>`, where each `ReadResult` contains an `XmlReader` positioned at the matching node, expected XPath from the attribute that, was encountered, and actual XPath of the returned node.

# Limitations
- Only simple XPath expressions are supported (e.g., no predicates, functions, or axes).
- Only absolute paths starting from the root are supported.
- When XPath contains an axis, the error is thrown during compilation.
- When XPath contains predicates, they are ignored, and only the node name is considered. 
- Please note that the generator adds a field (static or not) into the declared class. Therefore, interfaces and read-only structs only supported in static context.

Predicates support is planned for future releases.