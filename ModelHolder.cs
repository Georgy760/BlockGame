using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelHolder : MonoBehaviour
{
    public List<GameObject> allModels;
    [SerializeField]
    int activeModelIdx=0;

    public void Start()
    {
        SetModel(activeModelIdx);
    }

    void SetModel(int idx){
        for(int i = 0; i< allModels.Count;i++){
            if(i== idx){
                allModels[i].SetActive(true);
            }else{
                allModels[i].SetActive(false);
            }
        }
      
    }
    public void NextModel(){
        activeModelIdx++;
        activeModelIdx%=allModels.Count;
        SetModel(activeModelIdx);
    }

}
