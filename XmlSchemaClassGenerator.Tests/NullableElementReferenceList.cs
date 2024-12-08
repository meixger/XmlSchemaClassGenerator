using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace XmlSchemaClassGenerator.Tests;

public class NullableElementReferenceListTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var mow = new MemoryOutputWriter();
        var gen = new Generator()
        {
            OutputWriter = mow,
            Log = testOutputHelper.WriteLine,
            GenerateNullables = true,
            EnableNullableReferenceAttributes = true,
            GenerateInterfaces = false,
            CompactTypeNames = true,
            CollectionType = typeof(Array),
            CollectionSettersMode = CollectionSettersMode.PublicWithoutConstructorInitialization,
            UseShouldSerializePattern = true,
            SerializeEmptyCollections = true
        };
        gen.Generate(GetXmlSchemaSet(Schema));

        // log generated code
        foreach (var line in mow.Content.SelectMany(l => l.Split(Environment.NewLine))) testOutputHelper.WriteLine(line);

        var assembly = Compiler.Compile(nameof(Test), mow.Content.ToArray());
        void assertNullable(string typename, bool nullable)
        {
            Type c = assembly.GetType(typename);
            var property = c.GetProperty("Text");
            var setParameter = property.SetMethod.GetParameters();
            var getReturnParameter = property.GetMethod.ReturnParameter;
            var allowNullableAttribute = setParameter.Single().CustomAttributes.SingleOrDefault(a => a.AttributeType == typeof(AllowNullAttribute));
            var maybeNullAttribute = getReturnParameter.CustomAttributes.SingleOrDefault(a => a.AttributeType == typeof(MaybeNullAttribute));
            var hasAllowNullAttribute = allowNullableAttribute != null;
            var hasMaybeNullAttribute = maybeNullAttribute != null;
            Assert.Equal(nullable, hasAllowNullAttribute);
            Assert.Equal(nullable, hasMaybeNullAttribute);
        }
        assertNullable("SampleNamespace.ElementReferenceList", true);
    }

    const string Schema = """
                          <?xml version="1.0" encoding="UTF-8"?>
                          <xs:schema xmlns="SampleNamespace" targetNamespace="SampleNamespace" xmlns:xs="http://www.w3.org/2001/XMLSchema" elementFormDefault="qualified">
                          	<xs:element name="ElementReferenceList">
                          		<xs:complexType>
                          			<xs:sequence>
                          				<xs:element name="Text" type="xs:string" minOccurs="0" maxOccurs="unbounded"/>
                          				<xs:element name="Todo" type="xs:string" minOccurs="0"/>
                          			</xs:sequence>
                          		</xs:complexType>
                          	</xs:element>
                          </xs:schema>
                          """;

    private XmlSchemaSet GetXmlSchemaSet(string xsd)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(xsd));
        var schema = XmlSchema.Read(ms, null)!;
        var set = new XmlSchemaSet();
        set.Add(schema);
        return set;
    }
}
