using System.Xml;
using System.Xml.Schema;

namespace SolidApi.Services;

public static class XmlValidationService
{
    public static bool ValidateXml(string xmlPath, string xsdPath)
    {
        bool isValid = true;

        XmlSchemaSet schemas = new XmlSchemaSet();
        schemas.Add("", xsdPath);

        XmlReaderSettings settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemas
        };

        settings.ValidationEventHandler += (sender, e) =>
        {
            isValid = false;
            Console.WriteLine(
                $"Validation Error: {e.Message}");
        };

        using XmlReader reader =
            XmlReader.Create(xmlPath, settings);

        while (reader.Read())
        {
        }

        return isValid;
    }
}
