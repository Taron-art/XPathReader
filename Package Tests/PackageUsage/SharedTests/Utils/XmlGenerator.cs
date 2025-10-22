using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SharedTests.Utils
{
    public static class XmlGenerator
    {
        /// <summary>
        /// Generate XML string with internal nodes named "a" and "b" (alternating by level)
        /// and leaf elements named "c". Depth is number of levels (root is level 1).
        /// The output contains exactly 'leaves' number of <c/> elements.
        /// </summary>
        public static string GenerateXml(int depth, int leaves)
        {
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(depth));
            if (leaves < 0) throw new ArgumentOutOfRangeException(nameof(leaves));

            static string NameForLevel(int level) => (level % 2 == 1) ? "a" : "b";

            // Create root (level 1)
            XElement root = new XElement(NameForLevel(1));

            // If depth == 1 then we want the leaves to be at level 1.
            // XML must have a single root, so place the 'leaves' as direct children named "c"
            // inside the root when depth == 1 (the root itself is level 1 and children are level 1 in this interpretation).
            if (depth == 1)
            {
                for (int i = 0; i < leaves; i++)
                    root.Add(new XElement("c"));
                return new XDocument(root).ToString();
            }

            // Build internal chain up to level (depth - 1).
            XElement current = root;
            for (int level = 2; level <= depth - 1; level++)
            {
                var next = new XElement(NameForLevel(level));
                current.Add(next);
                current = next;
            }

            // Attach leaves named "c" so they are at level == depth.
            for (int i = 0; i < leaves; i++)
            {
                var leaf = new XElement("c");
                leaf.SetValue("value");
                current.Add(leaf);
            }

            return new XDocument(root).ToString();
        }
    }
}
