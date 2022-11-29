#pragma warning disable IDE0161
// Example of ProblemSolver

#nullable disable
namespace CompetitiveVerifier
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    internal abstract class ProblemSolver
    {
        public abstract string Url { get; }
        public virtual double? Error => null;

        public abstract void Solve();
        public string ToJson()
        {
            using (var stream = new MemoryStream())
            using (var sw = new StringWriter())
            {
                var serializer = new DataContractJsonSerializer(typeof(JsonDataContract));
                serializer.WriteObject(stream, new JsonDataContract(this));
                var json = Encoding.UTF8.GetString(stream.ToArray());
                sw.Write(json);
                return sw.ToString();
            }
        }
        [DataContract]
        private struct JsonDataContract
        {
            [DataMember(Name = "type", IsRequired = true)]
            public string Type { set; get; } = "problem";
            [DataMember(Name = "problem", IsRequired = true)]
            public string Url { set; get; }
            [DataMember(Name = "command", IsRequired = true)]
            public string Command { set; get; }
            [DataMember(Name = "error", EmitDefaultValue = false)]
            public double? Error { set; get; }

            public JsonDataContract(ProblemSolver solver)
            {
                Type = "problem";
                Url = solver.Url;
                Command = $"dotnet {System.Reflection.Assembly.GetEntryAssembly().Location} {solver.GetType().FullName}";
                Error = solver.Error;
            }
        }
    }
}