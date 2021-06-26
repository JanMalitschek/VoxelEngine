using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;

public class XMLAbstraction{
    public class Node
    {
        public XmlDocument xml { get; private set; }
        public XmlNode node { get; private set; }
        public string Name { get; private set; }
        public string InnerText
        {
            get
            {
                return node.InnerText;
            }
        }
        public Node(XmlDocument xml, XmlNode parent, string name, string innerText = "")
        {
            this.xml = xml;
            node = this.xml.CreateElement(name);
            parent.AppendChild(node);
            if (innerText != string.Empty)
                node.InnerText = innerText;
            Name = name;
        }
        public Node(XmlDocument xml, XmlNode node)
        {
            this.xml = xml;
            this.node = node;
            Name = node.Name;
        }
        public Node AddNode(string name, string innerText = "")
        {
            return new Node(xml, node, name, innerText);
        }
        public Node GetNode(string xPath)
        {
            XmlNode n = node.SelectSingleNode(xPath);
            if (n != null)
                return new Node(xml, n);
            return null;
        }
        public List<Node> GetNodes(string xPath)
        {
            List<Node> nodes = new List<Node>();
            foreach (XmlNode n in node.SelectNodes(xPath))
                nodes.Add(new Node(xml, n));
            return nodes;
        }
        public void AddAttribute(string name, object value)
        {
            XmlAttribute attribute = xml.CreateAttribute(name);
            attribute.Value = value.ToString();
            node.Attributes.Append(attribute);
        }
        public string GetAttribute(string name)
        {
            return node.Attributes[name].Value;
        }
    }

    public XmlDocument xml { get; private set; }
    private XmlNode root;

    public XMLAbstraction(string name)
    {
        xml = new XmlDocument();
        root = xml.CreateElement(name);
        xml.AppendChild(root);
    }
    public XMLAbstraction(string name, string path)
    {
        xml = new XmlDocument();
        try
        {
            xml.Load(path);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return;
        }
    }

    public Node AddNode(string name, string innerText = "")
    {
        return new Node(xml, root, name, innerText);
    }

    public Node GetNode(string xPath)
    {
        XmlNode n = xml.SelectSingleNode(xPath);
        if (n != null)
            return new Node(xml, n);
        return null;
    }
    public List<Node> GetNodes(string xPath)
    {
        List<Node> nodes = new List<Node>();
        foreach (XmlNode n in xml.SelectNodes(xPath))
            nodes.Add(new Node(xml, n));
        return nodes;
    }

    public void Save(string path)
    {
        xml.Save(path);
    }
}