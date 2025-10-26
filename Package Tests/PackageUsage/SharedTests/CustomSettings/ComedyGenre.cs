using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

#nullable enable

namespace SharedTests.CustomSettings
{
    [XmlType("genre", Namespace = "http://example.org/comedy")]
    [XmlRoot("genre", Namespace = "http://example.org/comedy", IsNullable = false)]
    public class Genre
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("origin")]
        public string? Origin { get; set; }
    }

}
