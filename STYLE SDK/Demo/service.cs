using System.Threading.Tasks;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Accessibility;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Collections;

//Interacting with blockchain
public class service : MonoBehaviour
{
    public Item item;
    public GameObject items;
    public Loader loader;
    public GameObject scroll;
    public GameObject modelContainer;
    public GameObject model;
    public Camera cameraMain;
    public TrackballCamera cameraModel;

    async private void  Start()
    {
        loader.Show();

        List<Dictionary<string, object>> requestedNFTs = await SDK.getRequestedNFTsAPI(new List<string>(), new List<string>(), new List<string>());

        for (int i = 0; i < requestedNFTs.Count; i++)
        {
            Dictionary<string, object> data = requestedNFTs[i];

            Item _item = Instantiate(item, items.transform);
            _item.Initialize(data, loader, scroll, modelContainer, model, cameraMain, cameraModel);
        }

        EventTrigger trigger = modelContainer.GetComponent<EventTrigger>( );
		EventTrigger.Entry entry = new EventTrigger.Entry( );
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener( (eventData) => {
            if (((PointerEventData)eventData).button == PointerEventData.InputButton.Right) {
                print("there");
                foreach (Transform child in model.transform)
                {
                    Destroy(child.gameObject);
                }
                scroll.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
                modelContainer.transform.localScale = new UnityEngine.Vector3(0, 0, 0);
                cameraMain.enabled = true;
                cameraModel.GetComponent<Camera>().enabled = false;
            }
        } );
		trigger.triggers.Add( entry );

        loader.Unshow();
    }
}