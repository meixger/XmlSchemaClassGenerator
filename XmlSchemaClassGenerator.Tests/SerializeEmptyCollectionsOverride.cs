using Ganss.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace XmlSchemaClassGenerator.Tests;

public class SerializeEmptyCollectionsOverride(ITestOutputHelper testOutputHelper)
{
    /*
        // Actual generated code
        public virtual bool ShouldSerializeListOfStrings()
        {
            return ((this.ListOfStrings != null) 
                        && (this.ListOfStrings.Count != 0));
        }

        // Idea 1a - nullable field for setting the override

        [XmlIgnore]
        public Nullable<bool> SerializeListOfStringsOverride;

        public virtual bool ShouldSerializeListOfStrings()
        {
            return SerializeListOfStringsOverride ??
                    ((this.ListOfStrings != null) 
                        && (this.ListOfStrings.Count != 0));
        }

        // Idea 1b - method overload for setting the override

        public virtual void ShouldSerializeListOfStrings(bool override)
            => _serializeListOfStringsOverride = override;

        [XmlIgnore]
        private Nullable<bool> _serializeListOfStringsOverride;

        public virtual bool ShouldSerializeListOfStrings()
        {
            return this._serializeListOfStringsOverride ??
                    ((this.ListOfStrings != null) 
                        && (this.ListOfStrings.Count != 0));
        }

        // Idea 2 - Func for replacing the default implementation
        // Does not work, because initializer cannot access non-static member
        public virtual Func<bool> ShouldSerializeListOfStrings = () =>
        {
            return ((this.ListOfStrings != null)              // error on this
                        && (this.ListOfStrings.Count != 0));  // error on this
        }

        // Idea 4 - partial method
        // No multiple implementations possible
     */
    [Fact]
    public void SequenceMinOccursZero()
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
            CollectionType = typeof(List<>),
            CollectionSettersMode = CollectionSettersMode.PublicWithoutConstructorInitialization,
            UseShouldSerializePattern = true,
            SerializeEmptyCollections = false
        };

        // var files = Glob.ExpandNames("xsd/AlpineBits/PartialRatePlan.xsd");
        // gen.Generate(files);
        gen.Generate(GetXmlSchemaSet());

        // log to unit test
        var lines = mow.Content.ToArray();
        foreach (var line in lines)
        {
            foreach (var s in line.Split(Environment.NewLine))
            {
                testOutputHelper.WriteLine(s);
            }
        }

        // var assembly = Compiler.Generate("alpinebitsfoo",
        //     "xsd/AlpineBits/PartialRatePlan.xsd", new Generator
        //     {
        //         GenerateNullables = true,
        //         UseShouldSerializePattern = true,
        //         NamespaceProvider = new NamespaceProvider
        //         {
        //             GenerateNamespace = key => "Test"
        //         },
        //         // OutputWriter = mow,
        //         OutputFolder = @"c:\temp\",
        //     });

        // var type = assembly.GetType("Test.OtaHotelRatePlanNotifRq");
        //
        // var collectionProperties = type
        //     .GetProperties()
        //     .Select(p => (p.Name, p.PropertyType))
        //     .OrderBy(p => p.Name)
        //     .ToArray();
        //
        // var uniqueIdType = assembly.GetType("Test.OtaHotelRatePlanNotifRqUniqueId");
        //
        // Assert.Equal(new[]
        // {
        //     ("UniqueId", uniqueIdType),
        // }, collectionProperties);
    }

    private XmlSchemaSet GetXmlSchemaSet()
    {
        const string xsd = """
                           <?xml version="1.0" encoding="UTF-8"?>

                           <xs:schema xmlns="SampleNamespace" targetNamespace="SampleNamespace" xmlns:xs="http://www.w3.org/2001/XMLSchema">
                           
                            <xs:element name="TypeWithNullableList">
                               	<xs:complexType>
                               		<xs:sequence>
                               			<xs:element name="ListOfStrings" minOccurs="0" maxOccurs="unbounded" type="xs:string" />
                               		</xs:sequence>
                               	</xs:complexType>
                            </xs:element>

                           </xs:schema>
                           """;
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(xsd));
        var schema = XmlSchema.Read(ms, null)!;
        var set = new XmlSchemaSet();
        set.Add(schema);
        return set;
    }
}