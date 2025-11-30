using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Repositories;
using PowerBI_MCP.Handlers;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Data;
using System.Security.Principal;
using Newtonsoft.Json;

namespace PowerBI_MCP.Service
{
    public class DaxService : IDaxService
    {
        private readonly DaxHandler _daxHandler;
        public DaxService(DaxHandler daxHandler)
        {
            _daxHandler = daxHandler;
        }
        public string RunDaxQuery(string query, string modelName)
        {
            return _daxHandler.ExecuteQuery(query, modelName, "DAX");
        }

        public string RunMdxQuery(string query, string modelName)
        {
            return _daxHandler.ExecuteQuery(query, modelName, "MDX");
        }
        public string SaveDaxQuery(string queryName, string query)
        {
            return _daxHandler.SaveDaxQueryToFile(queryName, query);
        }
    }
}
