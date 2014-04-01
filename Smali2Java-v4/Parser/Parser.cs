using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Smali2Java_v4.Parser
{
    public class ParserSmali
    {
        public List<Line> Lines = new List<Line>();

        public void Parse(string filename)
        {
            foreach (String s in File.ReadAllLines(filename))
                if(!String.IsNullOrEmpty(s.Trim()))
                    Lines.Add(new Line(s));

            LineGroup lastGroup = new LineGroup();
            foreach (Line l in Lines.Where(x => x.Type != ELineType.Comment))
            {
                if (l.NewGroup)
                {
                    Console.WriteLine(lastGroup.ToString());
                    lastGroup = new LineGroup();
                }
                lastGroup.Add(l);
            }
        }
    }
}
