using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Angelfish.AfxSystem.A.Common.Serialization
{
    // An implementation of Dirk Riehle's serializer pattern
    // that uses the Newtonsoft JSON serializer to write the
    // objects out in JSON format. This implementation allows
    // for deep copying, and reference management so that the
    // same IAfxSerializable object is not serialized twice.
    public class AfxSerializer
    {
        /// <summary>
        /// The collection of services associated with the
        /// serializer, used to provide access to specific
        /// services that may be required by serializable
        /// objects during the serialization process.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        private class SerializationState
        {
            public Queue<Action> EnqueuedActions = new Queue<Action>();

            public Queue<object> EnqueuedObjects = new Queue<object>();

            public Dictionary<string, object> ResolvedObjects = new Dictionary<string, object>();

            public Dictionary<string, string> ResolvedMembers = new Dictionary<string, string>();
        }

        /// <summary>
        /// The serialization context that is passed to objects
        /// that support IAfxSerializable, so that they can write
        /// all of their own attributes out to the stream.
        /// </summary>
        private class ObjectWriter : IAfxObjectWriter
        {
            // The destination JSON object that all of the
            // instance's attributes will be written out to:
            public JObject Destination { get; private set; }

            // The serialization context that maintains state
            // information for the serialization process:
            public SerializationState SerializerState { get; private set; }


            public ObjectWriter(JObject destination, SerializationState context)
            {
                this.Destination = destination;
                this.SerializerState = context;
            }

            public void WriteObject(string name, object value)
            {
                // The custom type converter intercepts attempts to
                // serialize objects that support IAfxSerializable and
                // incorporates them into our internal process:
                var converters = new JsonConverter[] { new ObjectConverter(SerializerState) };

                // The JSON.Net serializer is used to serialize the
                // specified value into its corresponding representation
                // as JSON formatted text, then it is reconstituted into
                // an instance of a LINQ to JSON object that can then be
                // added as a child node of the destination object:
                this.Destination.Add(name, JToken.Parse(JsonConvert.SerializeObject(value, converters)));
            }
        }

        /// <summary>
        /// The serialization context which is passed to objects that
        /// support IAfxSerializable so that they can read all of their
        /// previously persisted attributes back from the stream.
        /// </summary>
        private class ObjectReader : IAfxObjectReader
        {
            public SerializationState _serializationState { get; private set; }

            public ObjectReader(SerializationState context)
            {
                _serializationState = context;
            }

            public T ReadObject<T>(string name)
            {
                // The custom type converter intercepts attempts to
                // serialize objects that support IAfxSerializable and
                // incorporates them into our internal process:
                var converters = new JsonConverter[] { new ObjectConverter(_serializationState) };
                if (_serializationState.ResolvedMembers.ContainsKey(name))
                {
                    string property = _serializationState.ResolvedMembers[name];
                    return JsonConvert.DeserializeObject<T>(property, converters);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            public void ReadObject<T>(string name, Action<T> completion = null)
            {
                // The custom type converter intercepts attempts to
                // serialize objects that support IAfxSerializable and
                // incorporates them into our internal process:
                var converters = new JsonConverter[] { new ObjectConverter(_serializationState) };
                if (_serializationState.ResolvedMembers.ContainsKey(name))
                {
                    string property = _serializationState.ResolvedMembers[name];

                    var instance = JsonConvert.DeserializeObject<T>(property, converters);
                    _serializationState.EnqueuedActions.Enqueue(new Action(() => completion(instance)));
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Implements an instance of a type converter for
        /// the Newtonsoft JSON serializer, which overrides
        /// default handling of any object that implements
        /// the IAfxSerializable interface.
        /// </summary>
        private class ObjectConverter : JsonConverter
        {
            private SerializationState _state { get; set; }

            public ObjectConverter(SerializationState state)
            {
                _state = state;
            }

            /// <summary>
            /// Tells the serializer whether or not this converter
            /// supports conversion to or from ths specified type.
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            public override bool CanConvert(Type objectType)
            {
                if (typeof(IAfxSerializable).IsAssignableFrom(objectType))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Handles custom serialization for objects that support 
            /// the IAfxSerialization interface by writing out the guid
            /// and type of the object, then adding the actual instance
            /// onto the pending stack in the serialization context.
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="value"></param>
            /// <param name="serializer"></param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var instance = value as IAfxSerializable;
                if (instance != null)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("guid");
                    writer.WriteValue(instance.Id);
                    writer.WritePropertyName("type");
                    writer.WriteValue(instance.GetType().AssemblyQualifiedName);
                    writer.WriteEndObject();

                    if (!_state.ResolvedObjects.ContainsKey(instance.Id.ToString()))
                    {
                        _state.EnqueuedObjects.Enqueue(instance);
                    }
                }
            }

            /// <summary>
            /// Handles custom deserialization for objects that support
            /// the IAfxSerializable interface, by reading the serialized
            /// guid and type back from JSON, creating an instance of the
            /// corresponding type, and then pushing the instance onto the
            /// pending items stack in the serialization context.
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="objectType"></param>
            /// <param name="existingValue"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                object result = null;

                JObject element = JObject.Load(reader);
                var referenceGuid = element["guid"].ToString();
                var referenceType = element["type"].ToString();

                // If an object that has this guid has already been deserialized
                // then just grab it from the stack of resolved object instances
                // and return it to the JSON deserializer:
                if (_state.ResolvedObjects.ContainsKey(referenceGuid))
                {
                    result = _state.ResolvedObjects[referenceGuid];
                }
                else
                {
                    // The serialized object has not actually been read from the
                    // stream yet, so a placeholder is created and inserted into
                    // the collection of resolved instances in the serialization
                    // context; the serializer will later find the actual object
                    // data in the stream it is processing, and will then locate
                    // this placeholder and finish the deserialization process.
                    result = CreateInstance(referenceGuid, referenceType);
                    _state.ResolvedObjects.Add(referenceGuid, result);
                }

                return result;
            }
        }

        public AfxSerializer(IServiceProvider services)
        {
            this.Services = services;
        }

        /// <summary>
        /// Serializes the specified object out to a file
        /// as JSON formatted data.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="root"></param>
        public void Serialize(string path, IAfxSerializable root)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                Serialize(stream, root);
            }
        }

        /// <summary>
        /// Serializes the specified object out to a stream
        /// as JSON formatted data.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="root"></param>
        public void Serialize(Stream stream, IAfxSerializable instance)
        {
            // The root node that will contain the entire object
            // graph that is serialized from the root instance:
            JObject document = new JObject();

            // The serialiation state encapsulates information that
            // needs to be tracked over the course of the process:
            var serializationState = new SerializationState();

            var instanceArray = new JArray();

            serializationState.EnqueuedObjects.Enqueue(instance);
            while (serializationState.EnqueuedObjects.Count > 0)
            {
                var serializable = (IAfxSerializable)serializationState.EnqueuedObjects.Dequeue();
                if (!serializationState.ResolvedObjects.ContainsKey(serializable.Id.ToString()))
                {
                    serializationState.ResolvedObjects.Add(serializable.Id.ToString(), serializable);
                }

                var instanceNode = new JObject();
                var instanceData = new JObject();

                instanceData.Add("guid", serializable.Id.ToString());
                instanceData.Add("type", serializable.GetType().AssemblyQualifiedName);

                var instanceBody = new JObject();
                var writer = new ObjectWriter(instanceBody, serializationState);
                serializable.Serialize(writer, this.Services);

                instanceData.Add("body", instanceBody);

                instanceNode.Add("object", instanceData);
                instanceArray.Add(instanceNode);
            }

            document.Add("graph", instanceArray);

            // Write the JSON document out to the stream:
            using (var textWriter = new StreamWriter(stream))
            {
                textWriter.WriteLine(document.ToString());
            }
        }

        /// <summary>
        /// Reads a serialized instance of a class back from
        /// the JSON formatted file at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IAfxSerializable Deserialize(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Deserialize(stream);
            }
        }

        /// <summary>
        /// Reads a serialized instance of a class back from
        /// the JSON formatted data in the specified stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public IAfxSerializable Deserialize(Stream stream)
        {
            using (var textReader = new StreamReader(stream))
            {
                // The result can't be assigned until the
                // first serialized object has been read from
                // the specified stream:
                IAfxSerializable result = null;

                var state = new SerializationState();

                JObject document = JObject.Parse(textReader.ReadToEnd());
                foreach (var component in document["graph"])
                {
                    // Retrieve the component instance from the
                    // current item in the components array:
                    var instanceNode = component["object"];

                    // Retrieve the serialized identifier:
                    var instanceGuid = instanceNode["guid"].ToString();

                    // Retrieve the serialized .NET type:
                    var instanceType = instanceNode["type"].ToString();

                    // Determine whether or not a placeholder has already
                    // been added to the serialization context for use when
                    // deserializing this instance:
                    IAfxSerializable serializable = null;
                    if (state.ResolvedObjects.ContainsKey(instanceGuid))
                    {
                        serializable = (IAfxSerializable)state.ResolvedObjects[instanceGuid];
                    }
                    else
                    {
                        serializable = (IAfxSerializable)CreateInstance(instanceGuid, instanceType);
                        state.ResolvedObjects.Add(instanceGuid, serializable);
                    }


                    // Attempt to recover all of the children that
                    // were serialized by this instance:
                    state.ResolvedMembers.Clear();
                    foreach (var property in instanceNode["body"])
                    {
                        var attributeNode = property as JProperty;

                        var attributeName = attributeNode.Name;
                        var attributeBody = attributeNode.Value.ToString();

                        // This is a bit of a hack to ensure that any
                        // serialized attributes that are strings can
                        // be deserialized properly by the reader:
                        if (attributeNode.Value.Type == JTokenType.String)
                        {
                            attributeBody = "\"" + attributeBody + "\"";
                        }

                        state.ResolvedMembers.Add(attributeName, attributeBody);
                    }

                    var reader = new ObjectReader(state);
                    serializable.Deserialize(reader, this.Services);

                    if (result == null)
                    {
                        result = serializable;
                    }
                }

                // After the entire object graph has been
                // deserialized, execute any completion actions
                // that were requested by objects when they were
                // being deserialized:
                foreach (var action in state.EnqueuedActions)
                {
                    action();
                }

                return result;
            }
        }

        /// <summary>
        /// Creates a new instance of an object and initializes it with
        /// the specified GUID. The serializer relies on the GUID in order
        /// to ensure that object refe
        /// rences in the serialization context are
        /// mapped back to the appropriate object instances.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object CreateInstance(string guid, string type)
        {
            // REC: The creation of new instances of any serializable
            // type requires that they be constructed with a reference
            // to a unique identifier for their instance:
            Guid instanceGuid = new Guid(guid);

            // REC: The creation of new instances of any serializable
            // type is accomplished by dynamically loading them using
            // the .NET Activator:
            Type instanceType = Type.GetType(type);

            // REC: Request that the Activator create a new instance
            // of the specified type, calling the constructor with the
            // instance identifier specified by the caller:
            return Activator.CreateInstance(instanceType, new object[] { instanceGuid });
        }
    }

    /// <summary>
    /// The serialization interface that is implemented by a 
    /// class to provide compatibility with the serializer.
    /// </summary>
    public interface IAfxSerializable
    {
        /// <summary>
        /// The unique identifier that represents the instance
        /// identifier for a specific instance of an object that
        /// implements the IAfxSerializable interface.
        /// </summary
        /// <returns>
        /// The GUID that was assigned to the class at the time of
        /// construction, or deserialization. 
        /// </returns>
        Guid Id { get; }

        /// <summary>
        /// Invoked by the serializer to request that the serializable
        /// object write itself out to the serialization context.
        /// </summary>
        void Serialize(IAfxObjectWriter serializer, IServiceProvider services);

        /// <summary>
        /// The Deserialize method is invoked by the serializer when the
        /// instance of the object is being read back in from a stream.
        /// </summary>
        /// <param name="serializer"></param>
        void Deserialize(IAfxObjectReader serializer, IServiceProvider services);
    }

    public interface IAfxObjectWriter
    {
        void WriteObject(string name, object value);
    }

    public interface IAfxObjectReader
    {
        T ReadObject<T>(string name);

        void ReadObject<T>(string name, Action<T> completion);

    }
}