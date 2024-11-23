using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
// Define the Message class
[Serializable]
public class Message
{
    public string role; // Role of the speaker (teacher or student)
    public string content; // Content of the message
}

// Define the ModelResponseForAgent class
[Serializable]
public class ModelResponseForAgent
{
    public string id; // ID of the 3D model
    public string name; // Name of the 3D model
    public string category; // Category of the 3D model
    public string url; // URL of the 3D model

    public string creator_name; // Name of the creator of the 3D model
}

// Define the OnboardingStateAction class
[Serializable]
public class OnboardingStateAction
{
    public string state_index; // Updated State Index
}



// Define the ImageResponseForAgent class
[Serializable]
public class ImageResponseForAgent
{
    public string id; // ID of the image
    public string name; // Name of the image
    public string url; // URL of the image

    public string description; // Description of the image
}

// Define the SpeechResponseForAgent class
[Serializable]
public class SpeechResponseForAgent
{
    public string role; // Role of the speaker (teacher or student)
    public string content; // Content of the speech
}

[Serializable]
public class SketchContext
{
    public string screenshot; // Screenshot of the user's sketch
}

[Serializable]
public class SketchTo3DInput
{
    public SketchContext sketch_context; // Sketch context of the conversation
}

[Serializable]
public class SketchTo3DResponseAction
{
    public List<ModelResponseForAgent> CREATE_3D_MODEL = new List<ModelResponseForAgent>(); // List of 3D models to create
}

[Serializable]
public class SketchTo3DResponse
{
    public SketchTo3DResponseAction actions; // Actions to be taken in response to the sketch
}

[Serializable]
public class SpaceContext
{
    public List<ModelResponseForAgent> models;
    public List<ImageResponseForAgent> images;

    public SpaceContext()
    {
        models = new List<ModelResponseForAgent>
        {
            new ModelResponseForAgent { id = "", name = "", category = "", url = "" }
        };
        images = new List<ImageResponseForAgent>
        {
            new ImageResponseForAgent { id = "", name = "", url = "" }
        };
    }
}




[Serializable]
public class PointAt
{
    public string id_model; // ID of the 3D model that the user is pointing at
    public string id_image; // ID of the image that the user is pointing at
    public string screenshot; // Screenshot of the user pointing at the 3D model or image
}

// Add GestureContext class
[Serializable]
public class GestureContext
{
    public PointAt point_at; // User gesture context
    public List<string> grabbed = new List<string>(); // List of IDs of 3D models that the user has grabbed
}


// Define the Context class
[Serializable]
public class Context
{
    public string user_context = ""; // User-specific content
    public SpaceContext space_context = new SpaceContext(); // Context related to the space, including models and images
    public string reference_context = ""; // Reference information like a textbook or a lecture
    public string general_context = ""; // General context of the conversation
    public GestureContext gesture_context;
}

[Serializable]
public class SlideModel
{
    public string name; // Name of the 3D model to search for
    public string category; // Category of the 3D model
}

[Serializable]
public class SlideImage
{
    public string name; // Name/query of the image to search for
    public string description; // Description of the image
}

// Add Slide class
[Serializable]
public class Slide
{
    public string title; // Title of the slide
    public string description; // Description/content of the slide
    public SlideModel model; // 3D model information for the slide
    public SlideImage image; // Image information for the slide
}

// Define the CombinedInput class
[Serializable]
public class CombinedInput
{
    public SketchContext sketch_context; // Context of the conversation
}

[Serializable]
public class SnippetCreatorInput
{
    public string guidelines; // Description of snippet creator required
}

[Serializable]
public class SnippetResponse
{
    public List<Slide> slides; // List of slides in the slideshow
}

// Define the AgentInput class
[Serializable]
public class ConversationState
{
    public SketchContext sketch_context; // Context of the conversation
     private static readonly ConversationState emptyState = new ConversationState
    {
        sketch_context = new SketchContext
        {
            screenshot = ""
        }
    };

    private static readonly ConversationState testState = new ConversationState
    {
        sketch_context = new SketchContext
        {
            screenshot = ""
        }
    };

    public ConversationState current_state; // Declare current_state as a field

    public ConversationState(ConversationState initial_state)
    {
        current_state = initial_state;
    }



    public ConversationState()
    {
        sketch_context = new SketchContext();
        current_state = this;
    }

    public ConversationState(bool isTest)
    {
        current_state = isTest ? testState : emptyState;
    }


}





// Define the ModelInput class
[Serializable]
public class ModelInput
{
    public string text; // Text to generate a 3D model from
}

// Define the ImageInput class
[Serializable]
public class ImageInput
{
    public string text; // Text to generate an image from
}

// Define the TextInput class
[Serializable]
public class TextInput
{
    public string text; // Text to generate text from
}

public static class DictionaryExtensions
{
    public static List<T> GetActions<T>(this Dictionary<string, List<object>> actions, string actionType) where T : class
    {
        if (actions.TryGetValue(actionType, out var actionList))
        {
            return actionList.Select(action => JsonUtility.FromJson<T>(JsonUtility.ToJson(action))).ToList();
        }
        return new List<T>();
    }
}


// // Example usage (not part of the schema)
// public class ExampleUsage : MonoBehaviour
// {
//     void Start()
//     {
//         // Example input
        

//         // Example output (you would typically deserialize this from JSON)
//         MainResponse exampleOutput = new MainResponse
//         {
//             teacher_response = new TeacherResponse
//             {
//                 content = new Message
//                 {
//                     role = "teacher",
//                     content = "Great question! The water cycle, also known as the hydrologic cycle, has four main stages: evaporation, condensation, precipitation, and collection. Let's break these down:\n\n1. Evaporation: This is when water turns from a liquid to a gas, usually due to heat from the sun.\n2. Condensation: As water vapor rises and cools, it forms clouds.\n3. Precipitation: When water droplets in clouds become heavy enough, they fall as rain, snow, or other forms of precipitation.\n4. Collection: Water that falls as precipitation is collected in various bodies like oceans, lakes, and rivers, or seeps into the ground to become groundwater.\n\nThe cycle then repeats continuously, moving water around the planet."
//                 }
//             },
//             agent_response = new AgentResponse
//             {
//                 actions = new Dictionary<string, List<object>>
//                 {
//                     {PossibleActions.CREATE_IMAGE, new List<object>
//                     {
//                         new ImageResponseForAgent {id = "1", name = "water_cycle_diagram", url = "https://example.com/water_cycle_diagram.jpg"},
//                         new ImageResponseForAgent {id = "2", name = "evaporation_illustration", url = "https://example.com/evaporation_illustration.png"}
//                     }},
//                     {PossibleActions.CREATE_3D_MODEL, new List<object>
//                     {
//                         new ModelResponseForAgent {id = "3", name = "water_cycle_3d_model", url = "https://example.com/water_cycle_3d_model.glb"}
//                     }}
//                 }
//             }
//         };
//     }
// }