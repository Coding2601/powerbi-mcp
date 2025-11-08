using PowerBI_MCP.DTO;

namespace PowerBI_MCP.Utils.Cache{
    public static class ModelCache
    {
        public static void Set(string key, ModelDocumentation value)
        {
            CacheHandler<string, ModelDocumentation>.Set(key, value);
        }
        public static ModelDocumentation? Get(string key){
            return CacheHandler<string, ModelDocumentation>.Get(key);
        }
        public static bool Remove(string key){
            return CacheHandler<string, ModelDocumentation>.Remove(key);
        }
        public static void Clear(){
            CacheHandler<string, ModelDocumentation>.Clear();
        }
    }
}