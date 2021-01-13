using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{
    //A prefab allows you to create, configure, and store a gameobject with all its components, values, and children as a reusable asset.
    //essentially a template you can use to create new instances in a scene
    [SerializeField]
    private GameObject afterImagePrefab;

    //store all objects we have made that are not currently active
    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    //singleton used to access our script from other scripts
    //A singleton is a pattern that ensures a class has only a single globally accessible instance available at all times. Similar to a regular static class but with some advantages
    //apparently controversial to use because of misuse and abuse, but useful in specific scenarios. Hopefully this is one
    public static PlayerAfterImagePool Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GrowPool();
    }

    private void GrowPool()
    {
        //we will just create 10 objects at a time
        for (int i = 0; i < 10; i++)
        {
            //var tells compiler to figure out what it should be when it compiles. This should tell it to be a gameobject
            var instanceToAdd = Instantiate(afterImagePrefab);
            //this lines makes the gameobject we create a child of the gameobject this script is attached to (AfterImage)
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        //add to the pool
        availableObjects.Enqueue(instance);
    }

    //other scripts will call this, hence it being public
    public GameObject GetFromPool()
    {
        if(availableObjects.Count == 0)
        {
            GrowPool();
        }

        //take from pool
        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        return instance;
    }
}
