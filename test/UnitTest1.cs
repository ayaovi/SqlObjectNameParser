using System;
using System.Collections.Generic;
using System.Linq;
using src;
using Xunit;

namespace test
{
  public class UnitTest1
  {
    [Theory]
    [InlineData(new []{"dbo.SomeObject"}, "dbo.SomeObject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "dbo.Someobject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "dbo.someObject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "dbo.someobject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "SomeObject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "Someobject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "someObject", true)]
    [InlineData(new []{"dbo.SomeObject"}, "someobject", true)]
    [InlineData(new []{"SomeObject"}, "dbo.SomeObject", true)]
    [InlineData(new []{"SomeObject"}, "dbo.Someobject", true)]
    [InlineData(new []{"SomeObject"}, "dbo.someObject", true)]
    [InlineData(new []{"SomeObject"}, "dbo.someobject", true)]
    [InlineData(new []{"[dbo].SomeObject"}, "DBO.SoMeObjecT", true)]
    [InlineData(new []{"[dbo].SomeObject"}, "dBo.SomEobject", true)]
    [InlineData(new []{"[dbo].SomeObject"}, "dbO.someObject", true)]
    [InlineData(new []{"[dbo].SomeObject"}, "Dbo.sOmeobJEct", true)]
    [InlineData(new []{"\"dbo\".SomeObject"}, "[DBO].SoMeObjecT", true)]
    [InlineData(new []{"[dbo].SomeObject"}, "\"dBo\".SomEobject", true)]
    [InlineData(new []{"dbo.[SomeObject.MORE]"}, "[dbO].[someObject.more]", true)]
    [InlineData(new []{"dbo.[SomeObject.MORE]"}, "[dbO].someObject.more", true)]
    [InlineData(new []{"[dbo].\"SomeObject.MoRE\""}, "Dbo.[sOmeobJEct.more]", true)]
    public void Contains_GivenObjectListAndTargetObject_ExpectResult(string[] strSqlObjectNames, string strTarget, bool expected)
    {
      //Arrange
      var sqlObjectNames = strSqlObjectNames.Select(Class1.StringToSqlObjectName);
      var target = Class1.StringToSqlObjectName(strTarget);

      //Act && Assert      
      Assert.True(expected == Class1.Contains(sqlObjectNames, target));
    }

    [Theory]
    [InlineData("dbo.SomeObject.obj", new []{"dbo", "SomeObject", "obj"})]
    [InlineData("[dbo].SomeObject.obj", new []{"dbo", "SomeObject", "obj"})]
    [InlineData("dbo.[SomeObject].obj", new []{"dbo", "SomeObject", "obj"})]
    [InlineData("[dbo].[SomeObject].obj", new []{"dbo", "SomeObject", "obj"})]
    [InlineData("[dbo.something].SomeObject.obj", new []{"dbo.something", "SomeObject", "obj"})]
    [InlineData("[dbo.something].[Some.Object.More].obj", new []{"dbo.something", "Some.Object.More", "obj"})]
    [InlineData("\"dbo\".\"SomeObject\".obj", new []{"dbo", "SomeObject", "obj"})]
    public void Parse_GivenDotDelimitedObjectName_ExpectListSeperatedByDot(string input, string[] expected)
    {
      //Arrange && Act
      var result = Class1.Parse(input);
      //Assert
      Assert.True(AreEqual(expected, result));
    }

    [Theory]
    [InlineData("[dbo].[SomeObject]", '.', new []{5})]
    [InlineData("he.is.a.mad.dog", '.', new []{2, 5, 7, 11})]
    [InlineData("he+is+a+mad.dog", '+', new []{2, 5, 7})]
    public void FindAll_GivenString_ExpectAllIndicesOfSpecifiedCharacter(string input, char target, int[] expected)
    {
      //Arrange && Act
      var result = input.FindAll(target);

      //Assert
      Assert.True(AreEqual(expected, result));
    }

    [Theory]
    [InlineData("[dbo.something].[SomeObject]", 5, new []{"[]"}, true)]
    [InlineData("dbo.something.SomeObject", 5, new []{"[]"}, false)]
    [InlineData("[dbo.something].[SomeObject]", 15, new []{"[]"}, false)]
    [InlineData("{dbo.something}.[SomeObject.x]", 5, new []{"[]", "{}"}, true)]
    [InlineData("{dbo.something}.[SomeObject.x]", 15, new []{"[]", "{}"}, false)]
    [InlineData("{dbo.something}.[SomeObject.x]", 25, new []{"[]", "{}"}, true)]
    public void HasCharacterBounded_GivenString_ExpectResult(string input, int charIndex, string[] strEncapsulators, bool expected)
    {
      //Arrange
      var encapsulators = strEncapsulators.Select(x => x.ToArray())
      .Select(x => Tuple.Create(x[0], x[1]));

      //Act && Assert
      Assert.True(expected == input.HasCharacterBounded(charIndex, encapsulators));
    }

    [Theory]
    [InlineData("[dbo.some].[Object.kfjf]", '.', '*', new []{"[]"}, "[dbo*some].[Object*kfjf]")]
    [InlineData("dbo.some.Object.kfjf", '.', '*', new []{"[]"}, "dbo.some.Object.kfjf")]
    public void ReplaceAllBoundedChar_GivenString_ExpectResult(string input, char target, char replacement, string[] strEncapsulators, string expected)
    {
      //Arrange
      var encapsulators = strEncapsulators.Select(x => x.ToArray())
                                          .Select(x => Tuple.Create(x[0], x[1]));

      //Act && Assert
      Assert.Equal(expected, input.ReplaceAllBoundedChar(target, encapsulators, replacement));
    }

    [Theory]
    [InlineData("\"dbo.something\".\"SomeObject\"", "\"\"", "[]", "[dbo.something].[SomeObject]")]
    [InlineData("{dbo.something}.SomeObject", "{}", "[]", "[dbo.something].SomeObject")]
    [InlineData("{dbo.something}.<SomeObject>", "<>", "{}", "{dbo.something}.{SomeObject}")]
    public void ReplaceEncapsulator_GivenString_ExpectResult(string input, string strOldEncapsulator, string strNewEncapsulator, string expected)
    {
      //Arrange
      var oldEncapsulators = Tuple.Create(strOldEncapsulator.First(), strOldEncapsulator.Last());
      var newEncapsulators = Tuple.Create(strNewEncapsulator.First(), strNewEncapsulator.Last());

      //Act && Assert
      Assert.True(expected == input.ReplaceEncapsulator(oldEncapsulators, newEncapsulators));
    }

    [Theory]
    [InlineData("{dbo.something}.{SomeObject", "{}", "[]", "missing closing encapsulator")]
    [InlineData("{dbo.something}.SomeObject}", "{}", "[]", "extra closing encapsulator")]
    public void ReplaceEncapsulator_GivenString_ExpectException(string input, string strOldEncapsulator, string strNewEncapsulator, string exceptionMessage)
    {
      //Arrange
      var oldEncapsulators = Tuple.Create(strOldEncapsulator.First(), strOldEncapsulator.Last());
      var newEncapsulators = Tuple.Create(strNewEncapsulator.First(), strNewEncapsulator.Last());

      //Act
      var exception = Assert.Throws<Exception>(() => {input.ReplaceEncapsulator(oldEncapsulators, newEncapsulators);});

      //Assert      
      Assert.Equal(exceptionMessage, exception.Message);
    }

    private bool AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result)
    {
      Assert.True(expected.Count() == result.Count());
      Assert.True(expected.All(x => result.Contains(x)));
      return true;
    }
  }
}