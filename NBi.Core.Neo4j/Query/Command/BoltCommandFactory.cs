using NBi.Core.Neo4j.Query.Client;
using NBi.Extensibility.Query;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using NBi.Extensibility;

namespace NBi.Core.Neo4j.Query.Command
{
    class BoltCommandFactory : ICommandFactory
    {
        public bool CanHandle(IClient client) => client is BoltClient;

        public ICommand Instantiate(IClient client, IQuery query)
            => Instantiate(client, query, null);

        public ICommand Instantiate(IClient client, IQuery query, ITemplateEngine engine)
        {
            if (!CanHandle(client))
                throw new ArgumentException();
            var statement = Instantiate(query, engine);
            return new BoltCommand(client.CreateNew() as ISession, statement);
        }

        protected Statement Instantiate(IQuery query, ITemplateEngine engine)
        {
            var parameters = new Dictionary<string, object>();
            foreach (var paramater in query.Parameters)
                parameters.Add(
                    RenameParameter(paramater.Name), 
                    GetParameterValue(
                        paramater.GetValue(), 
                        paramater.SqlType?.ToLowerInvariant()?.Trim() ?? string.Empty
                    ));

            var statementText = query.Statement;

            if (query.TemplateTokens != null && query.TemplateTokens.Count() > 0)
                statementText = ApplyVariablesToTemplate(engine, query.Statement, query.TemplateTokens);

            return new Statement(statementText, parameters);
        }

        private object GetParameterValue(object originalValue, string type)
        {
            switch (type)
            {
                case "integer": return Convert.ToInt64(originalValue, CultureInfo.InvariantCulture.NumberFormat);
                case "float": return Convert.ToDouble(originalValue, CultureInfo.InvariantCulture.NumberFormat);
                case "boolean": return Convert.ToBoolean(originalValue, CultureInfo.InvariantCulture.NumberFormat);
                default:
                    return originalValue;
            }
        }

        private string ApplyVariablesToTemplate(ITemplateEngine engine, string template, IEnumerable<IQueryTemplateVariable> variables)
        {
            var valuePairs = new List<KeyValuePair<string, object>>();
            foreach (var variable in variables)
                valuePairs.Add(new KeyValuePair<string, object>(variable.Name, variable.Value));
            return engine.Render(template, valuePairs);
        }

        protected virtual string RenameParameter(string originalName)
        {
            if (originalName.StartsWith("$"))
                return originalName.Substring(1, originalName.Length - 1);
            else
                return originalName;
        }
    }
}
