using UnityEngine;
using System;
using System.Collections.Generic;

public class SimpleCubeController : MonoBehaviour
{
    public int Speed;

    Dictionary<string, bool> InsideTaggedArea = new Dictionary<string, bool> { { "Trigger Area 1", default(bool) }, { "Trigger Area 2", default(bool) } };

    void Update()
    {
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-Vector3.right * Time.deltaTime * Speed);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.right * Time.deltaTime * Speed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * Speed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(-Vector3.forward * Time.deltaTime * Speed);
        }

        var tags = new List<string>(InsideTaggedArea.Keys);

        foreach (var tag in tags)
        {
            var isInside = AreaExtensions.IsPositionWithinAreaWithTag(tag, transform.position);

            if (isInside && !InsideTaggedArea[tag])
            {
                Debug.Log($"Just entered area with tag: {tag}");
                InsideTaggedArea[tag] = true;
                continue;
            }

            if (!isInside && InsideTaggedArea[tag])
            {
                Debug.Log($"Just left area with tag: {tag}");
                InsideTaggedArea[tag] = false;
                continue;
            }
        }
    }
}
