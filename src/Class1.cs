using System;
using System.Collections.Generic;
using System.Linq;

namespace src
{
  public static class Class1
  {
    public static IEnumerable<string> Parse(string input)
    {
      return input.ReplaceEncapsulator(Tuple.Create('\"', '\"'), Tuple.Create('[', ']'))
                  .ReplaceAllBoundedChar('.', new[] { Tuple.Create('[', ']') }, '*')
                  .Split(new[] { '.', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => x.Replace('*', '.'));
    }

    public static IEnumerable<int> FindAll(this string lhs, char target)
    {
      return lhs.ToCharArray()
      .Select((x, i) => new { character = x, index = i })
      .Where(x => x.character == target)
      .Select(x => x.index);
    }

    public static bool HasCharacterBounded(this string lhs, int charIndex, IEnumerable<Tuple<char, char>> encapsulators)
    {
      var encapsulatorsOpeners = encapsulators.Select(x => x.Item1);
      var encapsulatorsTerminators = encapsulators.Select(x => x.Item2);

      var characters = lhs.ToCharArray();

      for (int i = charIndex; i >= 0; i--)
      {
        if (encapsulatorsTerminators.Contains(characters[i])) return false;
        if (encapsulatorsOpeners.Contains(characters[i])) return true;
      }
      return false;
    }

    public static SqlObjectName StringToSqlObjectName(string input)
    {
      var s = Parse(input).ToArray();
      var sqlObjectName = new SqlObjectName();

      if (s.Length == 1)
      {
        sqlObjectName.ObjectName = s[0];
      }
      else if (s.Length == 2)
      {
        sqlObjectName.Schema = s[0];
        sqlObjectName.ObjectName = s[1];
      }
      else if (s.Length == 3)
      {
        sqlObjectName.Database = s[0];
        sqlObjectName.Schema = s[1];
        sqlObjectName.ObjectName = s[2];
      }
      else if (s.Length == 4)
      {
        sqlObjectName.Server = s[0];
        sqlObjectName.Database = s[1];
        sqlObjectName.Schema = s[2];
        sqlObjectName.ObjectName = s[3];
      }
      else
      {
        throw new Exception("too many attribute");
      }
      return sqlObjectName;
    }

    public static bool Contains(IEnumerable<SqlObjectName> objects, SqlObjectName target)
    {
      return objects.Contains(target);
    }

    public static string ReplaceAllBoundedChar(this string lhs, char target, IEnumerable<Tuple<char, char>> encapsulators, char replacement)
    {
      var indices = lhs.FindAll(target);
      return string.Concat(lhs.ToArray()
                              .Select((x, i) => new { character = x, index = i })
                            .Select(x =>
                            {
                              var character = x.character;
                              if (indices.Contains(x.index) && lhs.HasCharacterBounded(x.index, encapsulators))
                              {
                                character = replacement;
                              }
                              return character;
                            })
      );
    }

    public static string ReplaceEncapsulator(this string lhs, Tuple<char, char> oldEncapsulators, Tuple<char, char> newEncapsulators)
    {
      var characters = lhs.ToArray();
      var oldEncapsulatorsOpenerIndices = lhs.FindAll(oldEncapsulators.Item1).ToArray();
      var oldEncapsulatorsTerminatorIndices = lhs.FindAll(oldEncapsulators.Item2).ToArray();

      if (oldEncapsulatorsOpenerIndices.Length != oldEncapsulatorsTerminatorIndices.Length)
      {
        var part = (oldEncapsulatorsOpenerIndices.Length > oldEncapsulatorsTerminatorIndices.Length) ? "missing closing" : "extra closing";
        throw new Exception($"{part} encapsulator");
      }

      if (oldEncapsulators.Item1 == oldEncapsulators.Item2)
      {
        if (oldEncapsulatorsOpenerIndices.Length == oldEncapsulatorsTerminatorIndices.Length &&
        oldEncapsulatorsOpenerIndices.Length % 2 == 0)
        {
          for (int i = 0; i < oldEncapsulatorsOpenerIndices.Length; i += 2)
          {
            characters[oldEncapsulatorsOpenerIndices[i]] = newEncapsulators.Item1;
            characters[oldEncapsulatorsOpenerIndices[i + 1]] = newEncapsulators.Item2;
          }
          return string.Concat(characters);
        }
        else
        {
          var part = (oldEncapsulatorsOpenerIndices.Length > oldEncapsulatorsTerminatorIndices.Length) ? "missing closing" : "extra closing";
          throw new Exception($"{part} encapsulator");
        }
      }

      return string.Concat(characters.Select((x, i) => new { character = x, index = i })
                                    .Select(x =>
                                    {
                                      var character = x.character;
                                      if (oldEncapsulatorsOpenerIndices.Contains(x.index))
                                        character = newEncapsulators.Item1;
                                      else if (oldEncapsulatorsTerminatorIndices.Contains(x.index))
                                        character = newEncapsulators.Item2;
                                      return character;
                                    })
      );
    }
  }

  public class SqlObjectName
  {
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public string ObjectName { get; set; } = string.Empty;

    public override bool Equals(object obj)
    {
      var sqlObjectName = obj as SqlObjectName;
      return ObjectName.ToLower() == sqlObjectName.ObjectName.ToLower() &&
      Schema.ToLower() == sqlObjectName.Schema.ToLower() &&
      Database.ToLower() == sqlObjectName.Database.ToLower() &&
      Server.ToLower() == sqlObjectName.Server.ToLower();
    }

    public override string ToString()
    {
      var name = Schema + ObjectName;
      if (!string.IsNullOrEmpty(Database)) name = Database + name;
      if (!string.IsNullOrEmpty(Server)) name = Server + name;
      return name;
    }
  }
}