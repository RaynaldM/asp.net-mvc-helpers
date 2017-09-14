using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace netcore.helpers
{
    /// <summary>
    /// Convert a JSON entity to an Interface definition for a model
    /// </summary>
    /// <example>
    ///     [JsonConverter(typeof(EntityModelConverter Item,IItem))]
    ///     public IItem MyItem { get; set; }
    /// </example>
    /// <see>
    ///     <cref>https://stackoverflow.com/questions/18324070/web-api-model-binding-to-an-interface</cref>
    /// </see>
    /// <typeparam name="TModel">Implemention of Model</typeparam>
    /// <typeparam name="TInterface">Interface of Model</typeparam>
    public class EntityModelConverter<TModel, TInterface> : JsonConverter where TModel : TInterface
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TInterface));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<TModel>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(TModel));
        }
    }

    /// <summary>
    /// Convert a collection JSON entity to an Interface definition for a model
    /// </summary>
    /// <example>
    ///     [JsonConverter(typeof(CollectionEntityConverter Item,IItem))]
    ///     public IList IItem  MyItem { get; set; }
    /// </example>
    /// <see cref="https://stackoverflow.com/questions/18324070/web-api-model-binding-to-an-interface"/>
    /// <typeparam name="TModel">Implemention of Model</typeparam>
    /// <typeparam name="TInterface">Interface of Model</typeparam>
    public class CollectionEntityConverter<TModel, TInterface> : JsonConverter where TModel : TInterface
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IList<TInterface>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IList<TInterface> items = serializer.Deserialize<List<TModel>>(reader).Cast<TInterface>().ToList();
            return items;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(IList<TModel>));
        }
    }
}
