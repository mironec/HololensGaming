using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PoseRunningAverage {
    Dictionary<int, PoseData>[] previousStates;
    int stateMemoryLength;
    int nextStateIdx = 0;
    
    public PoseRunningAverage(int _stateMemoryLength) {
        stateMemoryLength = _stateMemoryLength;
        previousStates = new Dictionary<int, PoseData>[stateMemoryLength];
        for(int i=0; i<stateMemoryLength; i++) {
            previousStates[i] = new Dictionary<int, PoseData>();
        }
    }

        //Updates newDict to the running-average values for each of the markers poses, taking this new data into account
    public void averageNewState(Dictionary<int, PoseData> newDict) {
        List<int> newDictKeys = new List<int>(newDict.Keys);
        previousStates[nextStateIdx].Clear();

        Vector3 totalPos = new Vector3();
        Quaternion totalRotation = new Quaternion(0, 0, 0, 0);

        int statesSeen;
        foreach(int key in newDictKeys) {
            PoseData newPose = newDict[key];
            previousStates[nextStateIdx][key] = newPose; //PoseData is a struct, so it will be copied to our previousStates dict. This allows us to modify newDict later

            int i = nextStateIdx;
            statesSeen = 0;
            totalPos.Set(0, 0, 0);
            totalRotation.Set(0, 0, 0, 0);

            do {
                if (!previousStates[i].ContainsKey(key)) break; //Only iterate while the dictionaries still contain this marker
                statesSeen++;
                PoseData previousPose = previousStates[i][key];
                totalPos += previousPose.pos;
                
                totalRotation.w += previousPose.rot.w;
                totalRotation.x += previousPose.rot.x;
                totalRotation.y += previousPose.rot.y;
                totalRotation.z += previousPose.rot.z;
                i = positiveMod(i - 1, stateMemoryLength);
            } while (i != nextStateIdx);

            totalPos /= statesSeen;
            newPose.pos = totalPos;

            //Average and normalise the total rotation. Quaternion average code taken from https://forum.unity3d.com/threads/average-quaternions.86898/
            totalRotation.x /= statesSeen;
            totalRotation.y /= statesSeen;
            totalRotation.z /= statesSeen;
            totalRotation.w /= statesSeen;

            //Normalize. Note: experiment to see whether you
            //can skip this step.
            float D = 1.0f / (totalRotation.w * totalRotation.w + totalRotation.x * totalRotation.x + totalRotation.y * totalRotation.y + totalRotation.z * totalRotation.z);
            totalRotation.x *= D;
            totalRotation.y *= D;
            totalRotation.z *= D;
            totalRotation.w *= D;

            newPose.rot = totalRotation;

            newDict[key] = newPose;
        }

        nextStateIdx = (nextStateIdx + 1) % stateMemoryLength;
    }

        //Computes x mod m, guaranteeing that the result is positive (i.e. -1 % 5 would be -1, but positiveMod(-1, 5) is 4
        //Taken from this SO answer: http://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
    int positiveMod(int x, int m) {
        return (x % m + m) % m;
    }
}
