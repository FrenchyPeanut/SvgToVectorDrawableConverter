﻿using System.Xml;
using JetBrains.Annotations;
using SvgToVectorDrawableConverter.DataFormat.Utils;

// ReSharper disable PossibleNullReferenceException

namespace SvgToVectorDrawableConverter.Utils
{
    static class SvgPreprocessor
    {
        public static void Preprocess([NotNull] string inputFileName, [NotNull] string outputFileName)
        {
            var xmlDocument = new XmlDocument { XmlResolver = null };
            xmlDocument.Load(inputFileName);

            WrapSvgContentInG(xmlDocument);
            ReplaceWithG(xmlDocument.DocumentElement, "defs");
            ReplaceWithG(xmlDocument.DocumentElement, "clipPath");
            SvgUseElementInliner.InlineUses(xmlDocument);

            xmlDocument.Save(outputFileName);
        }

        private static void WrapSvgContentInG(XmlDocument xmlDocument)
        {
            var svg = xmlDocument.DocumentElement;
            var g = xmlDocument.CreateElement("g", svg.NamespaceURI);
            while (svg.HasChildNodes)
            {
                g.AppendChild(svg.FirstChild);
            }
            svg.AppendChild(g);
        }

        private static void ReplaceWithG(XmlNode xmlNode, string nodeName)
        {
            for (var i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                var childNode = xmlNode.ChildNodes[i];
                ReplaceWithG(childNode, nodeName);
                if (childNode.Name != nodeName)
                {
                    continue;
                }

                var gNode = childNode.OwnerDocument.CreateElement("g", childNode.NamespaceURI);
                foreach (XmlAttribute attribute in childNode.Attributes)
                {
                    gNode.SetAttributeNode((XmlAttribute)attribute.Clone());
                }
                gNode.SetAttribute("display", "none");
                while (childNode.HasChildNodes)
                {
                    gNode.AppendChild(childNode.FirstChild);
                }

                xmlNode.ReplaceChild(gNode, childNode);
            }
        }
    }
}
