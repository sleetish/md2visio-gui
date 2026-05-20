using System;
using System.Text;
using md2visio.struc.figure;

namespace md2visio {
class Program {
    static void Main() {
        var sb = new StringBuilder();
        for (int i=0; i<10000; i++) {
            sb.Append("{\"a\":");
        }
        sb.Append("\"b\"");
        for (int i=0; i<10000; i++) {
            sb.Append("}");
        }
        try {
            var obj = new MmdJsonObj(sb.ToString());
            Console.WriteLine("Parsed!");
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
}
