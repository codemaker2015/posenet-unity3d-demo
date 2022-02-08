using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour {

	public static bool COMPLETED;
	//Analytics
	public static int ANALYTICS_DATA_UL, ANALYTICS_DATA_UM, ANALYTICS_DATA_UR, ANALYTICS_DATA_UF, ANALYTICS_DATA_UB, ANALYTICS_DATA_LL, ANALYTICS_DATA_LM, ANALYTICS_DATA_LR, ANALYTICS_DATA_LF,ANALYTICS_DATA_LB;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void ClearAnalytics() {
		ANALYTICS_DATA_UL = 0;
		ANALYTICS_DATA_UM = 0;
		ANALYTICS_DATA_UR = 0;
		ANALYTICS_DATA_UF = 0;
		ANALYTICS_DATA_UB = 0;
		ANALYTICS_DATA_LL = 0;
		ANALYTICS_DATA_LM = 0;
		ANALYTICS_DATA_LR = 0;
		ANALYTICS_DATA_LF = 0;
		ANALYTICS_DATA_LB = 0;
	}
}
