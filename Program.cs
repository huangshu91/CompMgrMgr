using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace USDCEntry
{
  class Program
  {
    static int eventId = 263;

    static void Main(string[] args)
    {
      WebRequest request = WebRequest.Create($"https://ndcapremier.com/feed/heatlists/?cyi={eventId}");
      WebResponse response = request.GetResponse();

      string URL = $"https://ndcapremier.com/feed/heatlists/?cyi={eventId}&id={{0}}&type=Attendee";

      Dictionary<int, string> eid_Name = new Dictionary<int, string>();
      Dictionary<string, int> eid_NameEvent = new Dictionary<string, int>();
      Dictionary<string, int> eId_comps = new Dictionary<string, int>();
      Dictionary<string, List<string>> eId_People = new Dictionary<string, List<string>>();


      string res;
      using (var sr = new StreamReader(response.GetResponseStream()))
      {
        res = sr.ReadToEnd();
      }

      JObject jo = JObject.Parse(res);
      var Entries = jo["Result"];

      var totalEntries = Entries.Count<JToken>();
      Console.WriteLine("total entries: " + totalEntries);

      double count = 0;

      foreach (var ent in Entries)
      {
        var competitor = ent;
        var competitorId = competitor["ID"];

        Console.WriteLine($"done {count}/{totalEntries}.");

        WebRequest request2 = WebRequest.Create(string.Format(URL, competitorId));
        WebResponse response2 = request2.GetResponse();

        count += 1;

        string res2;
        using (var sr = new StreamReader(response2.GetResponseStream()))
        {
          res2 = sr.ReadToEnd();
        }

        JObject jo2 = JObject.Parse(res2);

        var status = int.Parse((string) jo2["Status"]);
        if (status == 0) continue;

        var entries = jo2["Result"]["Entries"];
        //var entries = jo2["Result"];
        var name = jo2["Result"]["Name"];
        //var name = competitor["Name"];
        var nameComb = string.Empty;

        foreach (var n in name)
        {
          nameComb += n + " ";
        }

        foreach (var entry in entries)
        {
          var events = entry["Events"];
          foreach (var ev in events)
          {
            var evId = ev["Event_ID"].ToObject<int>();
            if (!eid_Name.ContainsKey(evId))
            {
              var evName = ev["Event_Name"].ToObject<string>();
              eid_Name.Add(evId, evName);
              eid_NameEvent.Add(evName, evId);
              eId_comps.Add(evName, 0);
              eId_People.Add(evName, new List<string>());
            }

            var evName2 = eid_Name[evId];
            eId_comps[evName2]++;
            eId_People[evName2].Add(nameComb);
          }
        }

      }

      Console.WriteLine();
      Console.WriteLine();

      while (true)
      {
        Console.WriteLine("Print Events: e");
        Console.WriteLine("Print Event competitor count: ec-{eventId}");
        Console.WriteLine("Print Event Name to Event Id: ei-{partial event name}");
        Console.WriteLine("Print Event competitiors: ep-{eventId}");
        Console.WriteLine("Print Event Name for partial string: find-{partial event name}");

        var comm = Console.ReadLine();

        if (comm.Equals("e"))
        {
          foreach (var kv in eid_Name)
          {
            Console.WriteLine(kv.Key + " :: " + kv.Value);
          }
        }
        else if (comm.Contains("ec-"))
        {
          int eventId = Int32.Parse(comm.Substring(3));
          var eventName = eid_Name[eventId];

          Console.WriteLine("eventName :: " + eId_comps[eventName]);
        }
        else if (comm.Contains("ei-"))
        {
          string find = comm.Substring(5);

          foreach (var kv in eid_NameEvent)
          {
            string compName = kv.Key;
            if (compName.ToLower().Contains(find.ToLower()))
            {
              Console.WriteLine("eventName :: " + kv.Key + " - eventId :: " + kv.Value);
            }
          }
        }  
        else if (comm.Contains("ep-"))
        {
          int eventId = Int32.Parse(comm.Substring(3));
          var eventName = eid_Name[eventId];

          var comps = eId_People[eventName];
          foreach (var p in comps)
          {
            Console.WriteLine(p);
          }
        }
        else if (comm.Contains("find-"))
        {
          string find = comm.Substring(5);

          foreach (var kv in eId_comps)
          {
            string compName = kv.Key;
            if (compName.ToLower().Contains(find.ToLower()))
            {
              Console.WriteLine(kv.Value + " :: " + kv.Key);
            }
          }
        }
      }
    }
  }
}
