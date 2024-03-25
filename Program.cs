using System.Xml;
using System.Xml.Schema;

class XMLVAlidator
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    XMLValidator.exe <XML name> [<XSD name>]");
            return 1;
        }

        string XmlFile = args[0];
        Console.WriteLine("XML file:");
        Console.WriteLine($"    {XmlFile}");

        try
        {
            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler((object? sender, ValidationEventArgs validationArgs) =>
            {
                if (validationArgs.Severity == XmlSeverityType.Warning) {
                    Console.WriteLine($"WARNING {validationArgs.Exception.LineNumber}, {validationArgs.Exception.LinePosition}: {validationArgs.Message}");
                }
                else
                    Console.WriteLine($"ERROR {validationArgs.Exception.LineNumber}, {validationArgs.Exception.LinePosition}: {validationArgs.Message}");
            });

            Console.WriteLine("XSD schemas:");
            for (int i = 1; i < args.Length; i++)
            {
                settings.Schemas.Add(String.Empty, args[i]);
                Console.WriteLine($"    {args[i]}");
            }

            using (var reader = XmlReader.Create(XmlFile))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.MoveToFirstAttribute())
                        {
                            do
                            {
                                if (reader.NamespaceURI == "http://www.w3.org/2001/XMLSchema-instance")
                                {
                                    if (reader.LocalName == "schemaLocation")
                                    {
                                        var parts = reader.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                        for (int i = 0; i < parts.Length; i += 2)
                                        {
                                            var destination = parts[i + 1];
                                            if (!Path.IsPathFullyQualified(destination))
                                            {
                                                destination = Path.Combine(Environment.CurrentDirectory, destination);
                                            }

                                            settings.Schemas.Add(parts[i], destination);
                                            Console.WriteLine($"    {parts[i]}={destination}");
                                        }
                                    }
                                    else if (reader.LocalName == "noNamespaceSchemaLocation")
                                    {
                                        var destination = reader.Value;
                                        if (!Path.IsPathFullyQualified(destination))
                                        {
                                            destination = Path.Combine(Environment.CurrentDirectory, destination);
                                        }

                                        settings.Schemas.Add(String.Empty, destination);
                                        Console.WriteLine($"    {destination}");
                                    }
                                }
                            } while (reader.MoveToNextAttribute());
                        }
                    }
                }
            }
            using (var reader = XmlReader.Create(XmlFile, settings))
            {
                while (reader.Read()) { };
            }
        }
        catch (Exception error)
        {
            Console.WriteLine(error.Message);
            return 1;
        }

        return 0;
    }
}
