import csv
import os
from mutagen.mp3 import MP3
import sys
import pandas as pd
from enum import Enum
import matplotlib.pyplot as plt
import numpy as np
import copy
import PIL.Image
import PIL.PngImagePlugin
import json

class Component(Enum):
    Hoist=1
    Trolley=2
    Pivot=3

    def GetDirName(self):
        match self:
            case Component.Hoist:
                return '1_Hoist'
            case Component.Trolley:
                return '2_Trolley'
            case Component.Pivot:
                return '3_Pivot'
            
class AnalyseResult:
    def __init__(self, totDur,NNVAT, IT, RTPlusNTT,IWT,genIdleClips,genReworkClipsDict,genInefficientClips):
        self.totDur=totDur
        self.NNVAT=NNVAT
        self.IT=IT
        self.RTPlusNTT=RTPlusNTT
        self.genIdleClips=genIdleClips
        self.genReworkClipsDict=genReworkClipsDict
        self.genInefficientClips=genInefficientClips
        self.VAT=totDur-NNVAT-IT-RTPlusNTT
        self.IWT=IWT
        self.WT=IT+RTPlusNTT+IWT
        WC=''
        if IT>0:
            WC+='A'
        if RTPlusNTT>0:
            WC+='BC'
        if IWT>0:
            WC+='D'
        self.WC=WC
        self.SV=0





if(len(sys.argv)<2):
    EXP_DIR_PATH = 'D:\\Projs\\VR_3.21\\Assets\\Record\\Exp\\Junyu\\test_11-20-01_2024-05-10_rTrain'
else:
    EXP_DIR_PATH = sys.argv[1]

print(EXP_DIR_PATH)

TC_DIR_PATH = os.path.join(EXP_DIR_PATH,'TC_player')
SUBJECT_DIR_PATH = os.path.abspath(os.path.join(EXP_DIR_PATH, os.pardir))
AUDIO_DIR_PATH = os.path.join(EXP_DIR_PATH,'Audios')
ALL_EXP_DIR_PATH = os.path.abspath(os.path.join(SUBJECT_DIR_PATH, os.pardir))

ACT_NUM=7

ACT_RECORD_END_KEYWORD_LIST=['SC_0','SC_1','SC_3','a4','SC_5','SC_9','SC_0']
ACT_RECORD_END_KEYWORD_INDEX_LIST=[0,0,0,0,0,0,1]
ACT_AUDIO_TAG_LIST=['a1','a2','a3','a4','a5','a6','a7']
ACT_COMPONENT_NUM_REQUIREMENT_LIST=[1,1,1,2,2,1,1]

MAX_SAFETY_HEIGHT=41.08
MIN_SAFETY_HEIGHT=36.08
TC_HEIGHT=63.08

ORI_VSM_PATH='D:\\Projs\\VR_3.21\Assets\\Record\\Exp\\visio_0416.png'

COMPONENT_IDLE_THRESHOLD={
    Component.Hoist:0.00015,
    Component.Trolley:0.0001,
    Component.Pivot:0.0003
}

COMPONENT_TARGET_STATUS={
    Component.Hoist:33.28,
    Component.Trolley:40.707,
    Component.Pivot:-16.381
}

def getActSuccess(i : int):
    for row in tasksData:
        if ACT_RECORD_END_KEYWORD_LIST[i]==row[0] and row[1]==True:
            return True
    return False
    
def getActAudioEndTs(i:int):
    lastTs=0
    for filename in os.listdir(AUDIO_DIR_PATH):
        f = os.path.join(AUDIO_DIR_PATH, filename)
        if os.path.isfile(f) and f.endswith('.mp3'):
            fTag =os.path.basename(f).replace('.mp3','').split('_')[0]

            if ACT_AUDIO_TAG_LIST[i]==fTag:
                ts =float(os.path.basename(f).replace('.mp3','').split('_')[-1])
                if(ts>lastTs):
                    lastTs=ts

    return lastTs

def getActAudioClips(i:int):
    audioClips=[]
    startTsToEndTsDict={}
    for filename in os.listdir(AUDIO_DIR_PATH):
        f = os.path.join(AUDIO_DIR_PATH, filename)
        if os.path.isfile(f) and f.endswith('.mp3'):
            fTag =os.path.basename(f).replace('.mp3','').split('_')[0]           
            if ACT_AUDIO_TAG_LIST[i]==fTag:
                endTs =float(os.path.basename(f).replace('.mp3','').split('_')[-1])
                startTs=float(os.path.basename(f).replace('.mp3','').split('_')[1])
                if startTs in startTsToEndTsDict.keys():
                    if startTsToEndTsDict[startTs]<endTs:
                        startTsToEndTsDict[startTs]=endTs
                else:
                    startTsToEndTsDict[startTs]=endTs
    for startTs in startTsToEndTsDict.keys():
        audioClips.append([startTs, startTsToEndTsDict[startTs]])
        i=0
    while(i<len(audioClips)-1):
        if audioClips[i][1]<=audioClips[i+1][0]:
            audioClips[i][1]=audioClips[i+1][1]
        else:
            i+=1

                

    return audioClips

def getActRecordEndTs(i:int):
    timeList=[]
    for row in tasksData:
        if ACT_RECORD_END_KEYWORD_LIST[i]==row[0]:
            timeList.append(row[2])

    if len(timeList)<=ACT_RECORD_END_KEYWORD_INDEX_LIST[i]:
        return float(tasksData[-1][2])
    else:
        return float(timeList[ACT_RECORD_END_KEYWORD_INDEX_LIST[i]])
    
def getActEndTs(i:int):
    audioEndTs=getActAudioEndTs(i)
    recordEndTs=getActRecordEndTs(i)

    return max(audioEndTs,recordEndTs)
def getAudioClips():
    audioClips=[]
    startTsToEndTsDict={}
    for filename in os.listdir(AUDIO_DIR_PATH):
        f = os.path.join(AUDIO_DIR_PATH, filename)
        if os.path.isfile(f) and f.endswith('.mp3'):
            endTs =float(os.path.basename(f).replace('.mp3','').split('_')[-1])
            startTs=float(os.path.basename(f).replace('.mp3','').split('_')[1])
            if startTs in startTsToEndTsDict.keys():
                if startTsToEndTsDict[startTs]<endTs:
                    startTsToEndTsDict[startTs]=endTs
            else:
                startTsToEndTsDict[startTs]=endTs
    for startTs in startTsToEndTsDict.keys():
        audioClips.append([startTs, startTsToEndTsDict[startTs]])
    audioClips.sort(key=lambda clip: clip[0])
    i=0
    while(i<len(audioClips)-1):
        if audioClips[i][1]>=audioClips[i+1][0]:
            audioClips[i][1]=audioClips[i+1][1]
            audioClips.pop(i+1)
        else:
            i+=1

    return audioClips

def getComponentIdleClips(component: Component):
    df=CBDfDict[component]
    df=df.loc[(df['behavName']=='UpdateStatus')]
    df.reset_index(inplace=True, drop=True)
    df=df.loc[abs(df['value'])<=COMPONENT_IDLE_THRESHOLD[component]]
    print(df)

    idleClips=[]
    lastIndex=0
    for index, row in df.iterrows():
        if len(idleClips)==0 or index!=lastIndex+1:
            idleClips.append([row['time'], row['time']])            
        else:
            idleClips[-1][1]=row['time']
        lastIndex=index

    return idleClips



def drawDistribution(xPoints, yPoints, savePath):
    plt.plot(xPoints, yPoints)
    plt.savefig(savePath)

def getComponentReworkClips(component):
    df=CADfDict[component]
    df=df.loc[(df['attriName']=='Status')]
    df.reset_index(inplace=True, drop=True)
    print(df)

    targetStatus= TC_HEIGHT-MAX_SAFETY_HEIGHT if component==Component.Hoist else COMPONENT_TARGET_STATUS[component]
    reworkClips=[]
    nearest=df.loc[0,'value']
    lastReworkIndex=0
    for index, row in df.iterrows():
        if component==Component.Hoist:
            if row['value']<=TC_HEIGHT-MAX_SAFETY_HEIGHT or (abs(targetStatus-nearest)<abs(targetStatus-row['value']) and TC_HEIGHT-MAX_SAFETY_HEIGHT<=row['value']<=TC_HEIGHT-MIN_SAFETY_HEIGHT):
                targetStatus=COMPONENT_TARGET_STATUS[component]
        if abs(targetStatus-nearest)<abs(targetStatus-row['value']):
            if lastReworkIndex+1!=index or len(reworkClips)==0:
                reworkClips.append([row['time'], row['time']])
            else:
                reworkClips[-1][1]=row['time']
            lastReworkIndex=index
        else:
            nearest=row['value']


    return reworkClips
def getClipsDuplicate(_aClips, _bClips):
    aClips=copy.deepcopy(_aClips)
    bClips=copy.deepcopy(_bClips)

    if len(aClips)==0 or len(bClips)==0:
        return []
    start=min(aClips[0][0],bClips[0][0])
    end=max(aClips[-1][1],bClips[-1][1])

    notBClips=[[start,end]]
    for clip in bClips:
        notBClips[-1][1]=clip[0]
        notBClips.append([clip[1],end])

    for clip in notBClips:
        if clip[0]==clip[1]:
            notBClips.remove(clip)

    return removeClipsDuplicate(aClips, notBClips)

def removeClipsDuplicate(_minuendClips, _subtrahendClips):
    minuendClips=copy.deepcopy(_minuendClips)
    subtrahendClips=copy.deepcopy(_subtrahendClips)

    i=0
    j=0
    while(i<len(minuendClips) and j<len(subtrahendClips)):
        mStart=minuendClips[i][0]
        sStart=subtrahendClips[j][0]
        mEnd=minuendClips[i][1]
        sEnd=subtrahendClips[j][1]
        if mStart<=sStart<mEnd or mStart<=sEnd<mEnd or sStart<=mStart<=mEnd<=sEnd:
            #have duplicate
            if mStart<=sStart<=sEnd<=mEnd:
                minuendClips[i][1]=sStart
                minuendClips.insert(i+1, [sEnd, mEnd])
                i+=1
                j+=1
            elif sStart<=mStart<=mEnd<=sEnd:
                minuendClips.pop(i)
            elif mStart<=sStart<mEnd:
                minuendClips[i][1]=sStart
                i+=1
            else:
                minuendClips[i][0]=sEnd
                j+=1
        else:
            if mStart<sStart:
                i+=1
            else:
                j+=1

    return minuendClips

def getClipsUnion(_aClips, _bClips):
    aClips=copy.deepcopy(_aClips)
    bClips=copy.deepcopy(_bClips)

    clips=aClips+bClips
    clips.sort(key=lambda clip: clip[0])

    i=0
    while(i<len(clips)-1 and len(clips)>1):
        if clips[i+1][0]<=clips[i][1] and clips[i+1][1]>clips[i][1]:
            clips[i][1]=clips[i+1][1]
            clips.pop(i+1)
        elif clips[i+1][0]<=clips[i][1] and clips[i+1][1]<=clips[i][1]:
            clips.pop(i+1)
        else:
            i+=1

    return clips

def getIntersection(_aClips,_bClip):
    aClips=copy.deepcopy(_aClips)
    bClip=copy.deepcopy(_bClip)

    intersection=[]
    for aClip in aClips:
        if bClip[0]<=aClip[0]<aClip[1]<bClip[1]:
            intersection.append(aClip)
        elif aClip[0]<=bClip[0]<aClip[1]<bClip[1]:
            intersection.append(aClip)
            intersection[-1][0]=bClip[0]
        elif bClip[0]<=aClip[0]<bClip[1]<aClip[1]:
            intersection.append(aClip)
            intersection[-1][1]=bClip[1]
    return intersection

def getClipsTotDur(_clips):
    clips=_clips

    totDur=0
    for clip in clips:
        totDur+=clip[1]-clip[0]

    return totDur

def getComponentResult(component:Component, startTs, endTs):
    audioClips=getAudioClips()
    audioClips=getIntersection(audioClips,[startTs,endTs])
    N_NVAT=getClipsTotDur(audioClips)

    idleClips=getIntersection(CIdleClipsDict[component],[startTs,endTs])
    idleDur=getClipsTotDur(idleClips)

    reworkClips=getIntersection(CIdleClipsDict[component],[startTs,endTs])
    reworkDur=getClipsTotDur(reworkClips)

def colormode255To1(color):
    return (color[0]/255, color[1]/255,color[2]/255)


def drawCalculationActResult():
    plt.figure(figsize=(6, 4),dpi=1000)
    plt.title("Time Clip Result Calculation")
    for component in (Component):
        for clip in CTaggedClipsDict[component]:
            color='black' if clip[2]=='idle' else 'red'
            plt.plot([clip[0],clip[1]], [component.name]*2, color=color, solid_capstyle="butt")
    print(CTaggedClipsDict)
    for i in range(ACT_NUM):
        plt.axvline(x = actClipList[i][0], color = 'green', linewidth=0.2, linestyle='--')
        plt.axvline(x = actClipList[i][0], color = 'green', linewidth=0.2, linestyle='--')
        plt.text(actClipList[i][0], 1.5, 'Act'+str(i+1), color='green', ha='left', va='top', rotation=90)
        for clip in actResults[i].genIdleClips:
            plt.plot([clip[0],clip[1]], ['Result']*2, color='black', solid_capstyle="butt")
            plt.axvline(x = clip[0], color = 'black', linewidth=0.2, linestyle='--')
            plt.axvline(x = clip[1], color = 'black', linewidth=0.2, linestyle='--')
        for k in actResults[i].genReworkClipsDict.keys():
            if k==1:
                color=colormode255To1((128, 0, 128))
            elif k==2/3:
                color=colormode255To1((255,0,0))
            elif k==1/2:
                color=colormode255To1((250, 160, 160))
            elif k==1/3:
                color=colormode255To1((255, 16, 240))     
            for clip in actResults[i].genReworkClipsDict[k]:
                plt.plot([clip[0],clip[1]], ['Result']*2, color=color, solid_capstyle="butt")
                plt.axvline(x = clip[0], color = color, linewidth=0.2, linestyle='--')
                plt.axvline(x = clip[1], color = color, linewidth=0.2, linestyle='--')
        for clip in actResults[i].genInefficientClips:
            color=colormode255To1((255, 204, 0))
            plt.plot([clip[0],clip[1]], ['Result']*2, color=color, solid_capstyle="butt")
            plt.axvline(x = clip[0], color = color, linewidth=0.2, linestyle='--')
            plt.axvline(x = clip[1], color = color, linewidth=0.2, linestyle='--')

    plt.tight_layout()
    plt.savefig(os.path.join(EXP_DIR_PATH,'Calculation_Result.png'))
    plt.clf()


def drawCalculationNNVAT():
    plt.figure(figsize=(6, 4),dpi=1000)
    plt.title("Audio, Raw Idle, Raw Rework and NNVAT")
    for clip in audioClips:
        plt.plot([clip[0],clip[1]],['0 Audio']*2, color='blue', solid_capstyle="butt")
        plt.axvline(x = clip[0], color = (0.1,0.1,0.7, 0.3), linewidth=0.2, linestyle='--')
        plt.axvline(x = clip[1], color = (0.1,0.1,0.7, 0.3), linewidth=0.2, linestyle='--')
    i=0
    for component in (Component):
        for clip in rawCIdleClipsDict[component]:
            plt.plot([clip[0],clip[1]], [str(i+1).zfill(1)+' Raw Idle and Rework of '+component.name]*2, color=('black', 0.75), solid_capstyle="butt")
        for clip in rawCReworkClipsDict[component]:
            plt.plot([clip[0],clip[1]], [str(i+1).zfill(1)+' Raw Idle and Rework of '+component.name]*2, color=('red', 0.75), solid_capstyle="butt")
        i+=1
    for clip in NNVATClips:
        plt.plot([clip[0],clip[1]], ['4 NNVAT']*2, color='blue', solid_capstyle="butt")
        plt.axvline(x = clip[0], color = 'blue', linewidth=0.2, linestyle='--')
        plt.axvline(x = clip[1], color = 'blue', linewidth=0.2, linestyle='--')
    plt.tight_layout()
    plt.savefig(os.path.join(EXP_DIR_PATH,'Calculation_NNVAT.png'))
    plt.clf()

def drawCalculationIdle():
    fig, axs = plt.subplots(len((Component)),figsize=(6, 12),dpi=1000)
    fig.suptitle("Component Raw Idle, Idle and NNVAT")
    i=0
    for component in (Component):
        axs[i].set_title(component.name)
        for clip in rawCIdleClipsDict[component]:
            axs[i].plot([clip[0],clip[1]], ['0 Raw Idle of '+component.name]*2, color='black', solid_capstyle="butt")
        for clip in NNVATClips:
            axs[i].plot([clip[0],clip[1]], ['1 NNVAT']*2, color='blue', solid_capstyle="butt")
        for clip in CIdleClipsDict[component]:
            axs[i].plot([clip[0],clip[1]], ['2 Idle of '+component.name]*2, color='black', solid_capstyle="butt")
            axs[i].axvline(x = clip[0], color = 'black', linewidth=0.2, linestyle='--')
            axs[i].axvline(x = clip[1], color = 'black', linewidth=0.2, linestyle='--')
        i+=1
    plt.tight_layout()
    fig.savefig(os.path.join(EXP_DIR_PATH,'Calculation_Idle.png'))
    plt.clf()

def drawCalculationRework():
    fig, axs = plt.subplots(len((Component)),figsize=(6, 12),dpi=1000)
    fig.suptitle("Component Raw Rework, Raw Idle, Rework and NNVAT")
    i=0
    for component in (Component):
        axs[i].set_title(component.name)
        for clip in rawCReworkClipsDict[component]:
            axs[i].plot([clip[0],clip[1]], ['0 Raw Rework of '+component.name]*2, color='red', solid_capstyle="butt")
        for clip in rawCIdleClipsDict[component]:
            axs[i].plot([clip[0],clip[1]], ['1 Raw Idle of '+component.name]*2, color='black', solid_capstyle="butt")
        for clip in NNVATClips:
            axs[i].plot([clip[0],clip[1]], ['2 NNVAT']*2, color='blue', solid_capstyle="butt")
        for clip in CReworkClipsDict[component]:
            axs[i].plot([clip[0],clip[1]], ['3 Rework of '+component.name]*2, color='red', solid_capstyle="butt")
            axs[i].axvline(x = clip[0], color = 'red', linewidth=0.2, linestyle='--')
            axs[i].axvline(x = clip[1], color = 'red', linewidth=0.2, linestyle='--')
        i+=1
    plt.tight_layout()
    fig.savefig(os.path.join(EXP_DIR_PATH,'Calculation_Rework.png'))
    plt.clf()

def drawCalculationComponent():
    fig, axs = plt.subplots(len((Component)),figsize=(5, 4),dpi=1000)
    i=0
    for component in (Component):
        axs[i].set_title(component.name)
        df=CADfDict[component]
        print(df)
        df=df.loc[(df['attriName']=='Status')]
        df.reset_index(inplace=True, drop=True)
        print(df)
        axs[i].plot(df['time'],df['value'], color='green', solid_capstyle="butt")

        for clip in NNVATClips:
            clipDf=df.loc[(clip[0]<=df['time']) & (df['time']<clip[1])]
            axs[i].plot(clipDf['time'], clipDf['value'], color='blue', solid_capstyle="butt")
        for clip in CIdleClipsDict[component]:
            clipDf=df.loc[(clip[0]<=df['time']) & (df['time']<clip[1])]
            axs[i].plot(clipDf['time'], clipDf['value'], color='black', solid_capstyle="butt")
        for clip in CReworkClipsDict[component]:
            clipDf=df.loc[(clip[0]<=df['time']) & (df['time']<clip[1])]
            axs[i].plot(clipDf['time'], clipDf['value'], color='red', solid_capstyle="butt")
        
        axs[i].axhline(y = COMPONENT_TARGET_STATUS[component], color = 'green',linewidth=0.2, linestyle = '-')
        if component==Component.Hoist:
            axs[i].axhline(y = TC_HEIGHT-MAX_SAFETY_HEIGHT, color = 'red',linewidth=0.2, linestyle = '-')
            axs[i].axhline(y = TC_HEIGHT-MIN_SAFETY_HEIGHT, color = 'red',linewidth=0.2, linestyle = '-')
        i+=1
    plt.tight_layout()
    fig.savefig(os.path.join(EXP_DIR_PATH,'Component.png'))
    plt.clf()

def drawVSM():
    image = plt.imread(ORI_VSM_PATH)
    fig, ax = plt.subplots()
    ax.imshow(image)

    # T1---------------------------------------------------------------
    text_VAT_1 = round(actResults[0].VAT, 1)
    x = 800  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_1, fontsize=3.5, color='darkblue')
    x = 550  # 文字所在的 x 坐标
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_1, fontsize=3.5, color='darkblue')

    s_VAT_1 = "s"
    x = 1150  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_1, fontsize=3.5, color='darkblue')

    text_WT_1 = round(actResults[0].WT, 1)
    x = 800  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_1, fontsize=3.5, color='darkblue')

    s_WT_1 = "s"
    x = 1150  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_1, fontsize=3.5, color='darkblue')

    text_nNVAT_1 = round(actResults[0].NNVAT, 1)
    x = 800  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_1, fontsize=3.5, color='darkblue')

    s_text_nNVAT_1 = "s"
    x = 1150  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_1, fontsize=3.5, color='darkblue')

    text_wc_1 = actResults[0].WC
    x = 800  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标3
    ax.text(x, y, text_wc_1, fontsize=3.5, color='darkblue')

    text_sv_1 = int(actResults[0].SV)
    x = 800  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_1, fontsize=3.5, color='darkblue')

    text_T1 = round(actResults[0].totDur, 1)
    x = 1400  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T1, fontsize=3.5, color='darkblue')

    # T2---------------------------------------------------------------
    text_VAT_2 = round(actResults[1].VAT, 1)
    x = 2450  # 文字所在的 x 坐标 1650
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_2, fontsize=3.5, color='darkblue')
    x = 2200  # 文字所在的 x 坐标 1650
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_2, fontsize=3.5, color='darkblue')

    s_VAT_2 = "s"
    x = 2750  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_2, fontsize=3.5, color='darkblue')

    text_WT_2 = round(actResults[1].WT, 1)
    x = 2450  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_2, fontsize=3.5, color='darkblue')

    s_WT_2 = "s"
    x = 2750  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_2, fontsize=3.5, color='darkblue')

    text_nNVAT_2 = round(actResults[1].NNVAT, 1)
    x = 2450  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_2, fontsize=3.5, color='darkblue')

    s_text_nNVAT_2 = "s"
    x = 2750  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_2, fontsize=3.5, color='darkblue')

    text_wc_2 = actResults[1].WC
    x = 2450  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标
    ax.text(x, y, text_wc_2, fontsize=3.5, color='darkblue')

    text_sv_2 = int(actResults[1].SV)
    x = 2450  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_2, fontsize=3.5, color='darkblue')

    text_T2 = round(actResults[1].totDur, 1)
    x = 3050  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T2, fontsize=3.5, color='darkblue')

    # T3---------------------------------------------------------------
    text_VAT_3 = round(actResults[2].VAT, 1)
    x = 4100  # 文字所在的 x 坐标 1650
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_3, fontsize=3.5, color='darkblue')
    x = 3850  # 文字所在的 x 坐标 1650
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_3, fontsize=3.5, color='darkblue')

    s_VAT_3 = "s"
    x = 4400  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_3, fontsize=3.5, color='darkblue')

    text_WT_3 = round(actResults[2].WT, 1)
    x = 4100  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_3, fontsize=3.5, color='darkblue')

    s_WT_3 = "s"
    x = 4400  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_3, fontsize=3.5, color='darkblue')

    text_nNVAT_3 = round(actResults[2].NNVAT, 1)
    x = 4100  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_3, fontsize=3.5, color='darkblue')

    s_text_nNVAT_3 = "s"
    x = 4400  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_3, fontsize=3.5, color='darkblue')

    text_wc_3 = actResults[2].WC
    x = 4100  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标
    ax.text(x, y, text_wc_3, fontsize=3.5, color='darkblue')

    text_sv_3 = int(actResults[2].SV)
    x = 4100  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_3, fontsize=3.5, color='darkblue')

    text_T3 = round(actResults[2].totDur, 1)
    x = 4650  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T3, fontsize=3.5, color='darkblue')

    # T4---------------------------------------------------------------
    text_VAT_4 = round(actResults[3].VAT, 1)
    x = 5750  # 文字所在的 x 坐标 1650
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_4, fontsize=3.5, color='darkblue')
    x = 5500  # 文字所在的 x 坐标 1650
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_4, fontsize=3.5, color='darkblue')

    s_VAT_4 = "s"
    x = 6050  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_4, fontsize=3.5, color='darkblue')

    text_WT_4 = round(actResults[3].WT, 1)
    x = 5750  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_4, fontsize=3.5, color='darkblue')

    s_WT_4 = "s"
    x = 6050  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_4, fontsize=3.5, color='darkblue')

    text_nNVAT_4 = round(actResults[3].NNVAT, 1)
    x = 5750  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_4, fontsize=3.5, color='darkblue')

    s_text_nNVAT_4 = "s"
    x = 6050  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_4, fontsize=3.5, color='darkblue')

    text_wc_4 = actResults[3].WC
    x = 5750  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标
    ax.text(x, y, text_wc_4, fontsize=3.5, color='darkblue')

    text_sv_4 = int(actResults[3].SV)
    x = 5750  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_4, fontsize=3.5, color='darkblue')

    text_T4 = round(actResults[3].totDur, 1)
    x = 6300  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T4, fontsize=3.5, color='darkblue')

    # T5---------------------------------------------------------------
    text_VAT_5 = round(actResults[4].VAT, 1)
    x = 7400  # 文字所在的 x 坐标 1650
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_5, fontsize=3.5, color='darkblue')
    x = 7150  # 文字所在的 x 坐标 1650
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_5, fontsize=3.5, color='darkblue')

    s_VAT_5 = "s"
    x = 7700  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_5, fontsize=3.5, color='darkblue')

    text_WT_5 = round(actResults[4].WT, 1)
    x = 7400  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_5, fontsize=3.5, color='darkblue')

    s_WT_5 = "s"
    x = 7700  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_5, fontsize=3.5, color='darkblue')

    text_nNVAT_5 = round(actResults[4].NNVAT, 1)
    x = 7400  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_5, fontsize=3.5, color='darkblue')

    s_text_nNVAT_5 = "s"
    x = 7700  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_5, fontsize=3.5, color='darkblue')

    text_wc_5 = actResults[4].WC
    x = 7400  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标
    ax.text(x, y, text_wc_5, fontsize=3.5, color='darkblue')

    text_sv_5 = int(actResults[4].SV)
    x = 7400  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_5, fontsize=3.5, color='darkblue')

    text_T5 = round(actResults[4].totDur)
    x = 7950  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T5, fontsize=3.5, color='darkblue')

    # T6---------------------------------------------------------------
    text_VAT_6 = round(actResults[5].VAT, 1)
    x = 9050  # 文字所在的 x 坐标 1650
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_6, fontsize=3.5, color='darkblue')
    x = 8800  # 文字所在的 x 坐标 1650
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_6, fontsize=3.5, color='darkblue')

    s_VAT_6 = "s"
    x = 9350  # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_6, fontsize=3.5, color='darkblue')

    text_WT_6 = round(actResults[5].WT, 1)
    x = 9050  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_6, fontsize=3.5, color='darkblue')

    s_WT_6 = "s"
    x = 9350  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_6, fontsize=3.5, color='darkblue')

    text_nNVAT_6 = round(actResults[5].NNVAT, 1)
    x = 9050  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_6, fontsize=3.5, color='darkblue')

    s_text_nNVAT_6 = "s"
    x = 9350  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_6, fontsize=3.5, color='darkblue')

    text_wc_6 = actResults[5].WC
    x = 9050  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标
    ax.text(x, y, text_wc_6, fontsize=3.5, color='darkblue')

    text_sv_6 = int(actResults[5].SV)
    x = 9050  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_6, fontsize=3.5, color='darkblue')

    text_T6 = round(actResults[5].totDur, 1)
    x = 9550  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T6, fontsize=3.5, color='darkblue')


    # T7---------------------------------------------------------------
    text_VAT_7 = round(actResults[6].VAT, 1)
    x = 10700  # 文字所在的 x 坐标 1650
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_7, fontsize=3.5, color='darkblue')
    x = 10450  # 文字所在的 x 坐标 1650
    y = 3500  # 文字所在的 y 坐标
    ax.text(x, y, text_VAT_7, fontsize=3.5, color='darkblue')

    s_VAT_7 = "s"
    x = 11000 # 文字所在的 x 坐标
    y = 2160  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_7, fontsize=3.5, color='darkblue')

    text_WT_7 = round(actResults[6].WT, 1)
    x = 10700  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, text_WT_7, fontsize=3.5, color='darkblue')

    s_WT_7 = "s"
    x = 11000  # 文字所在的 x 坐标
    y = 2360  # 文字所在的 y 坐标
    ax.text(x, y, s_WT_7, fontsize=3.5, color='darkblue')

    text_nNVAT_7 = round(actResults[6].NNVAT, 1)
    x = 10700  # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, text_nNVAT_7, fontsize=3.5, color='darkblue')

    s_text_nNVAT_7 = "s"
    x = 11000 # 文字所在的 x 坐标
    y = 2570  # 文字所在的 y 坐标
    ax.text(x, y, s_text_nNVAT_7, fontsize=3.5, color='darkblue')

    text_wc_7 = actResults[6].WC
    x = 10700  # 文字所在的 x 坐标
    y = 2780  # 文字所在的 y 坐标
    ax.text(x, y, text_wc_7, fontsize=3.5, color='darkblue')

    text_sv_7 = int(actResults[6].SV)
    x = 10700  # 文字所在的 x 坐标
    y = 2990  # 文字所在的 y 坐标
    ax.text(x, y, text_sv_7, fontsize=3.5, color='darkblue')

    final_output_total_VAT=0
    final_output_total_Dur=0
    for i in range(ACT_NUM):
        final_output_total_VAT+=actResults[i].VAT
        final_output_total_Dur+=actResults[i].totDur


    text_T7 = round(final_output_total_Dur, 1)
    x = 11350  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, text_T7, fontsize=3.5, color='darkblue')

    s_T7 = "s"
    x = 11700  # 文字所在的 x 坐标
    y = 3325  # 文字所在的 y 坐标
    ax.text(x, y, s_T7, fontsize=3.5, color='darkblue')

    

    text_output_total_VAT = round(final_output_total_VAT, 1)
    x = 11350  # 文字所在的 x 坐标
    y = 3575  # 文字所在的 y 坐标
    ax.text(x, y, text_output_total_VAT, fontsize=3.5, color='darkblue')

    s_VAT_T7 = "s"
    x = 11700  # 文字所在的 x 坐标
    y = 3575  # 文字所在的 y 坐标
    ax.text(x, y, s_VAT_T7, fontsize=3.5, color='darkblue')

    ax.axis('off')
    dpi = 1200  # 设置为所需的 DPI 值
    plt.savefig(os.path.join(EXP_DIR_PATH,'output_sub_VSM_plot'), dpi=dpi, bbox_inches='tight', pad_inches=0)


tasksData=[]
with open(os.path.join(EXP_DIR_PATH, 'tasks.csv')) as file:
    reader = csv.reader(file)
    for row in reader:
        tasksData.append(row)




actClipList=[]
fail=False
SVAct=-1
for i in range(ACT_NUM):
    actStartTs=0 if i==0 else actClipList[i-1][1]
    actEndTs=getActEndTs(i)
    actClipList.append([actStartTs,actEndTs])
    
    if not getActSuccess(i) and not fail:
        fail=True
        SVAct=i



CADfDict={}
CBDfDict={}
for component in (Component):
    CADfDict[component]=pd.read_csv(os.path.join(TC_DIR_PATH, component.GetDirName(),'attributes.csv'))
    CADfDict[component].columns=['attriName', 'time', 'value']
    CBDfDict[component]=pd.read_csv(os.path.join(TC_DIR_PATH, component.GetDirName(),'behaviors.csv'))
    CBDfDict[component].columns=['behavName', 'time', 'value']

print(CADfDict)
print(CBDfDict)


audioClips=getAudioClips()
rawCIdleClipsDict={}
rawCReworkClipsDict={}
CIdleClipsDict={}
CReworkClipsDict={}
CTaggedClipsDict={}
NNVATClips=copy.deepcopy(audioClips)


for component in (Component):
    rawCIdleClipsDict[component]=getComponentIdleClips(component)
    rawCReworkClipsDict[component]=getComponentReworkClips(component)

for component in (Component):
    NNVATClips=getClipsDuplicate(NNVATClips,getClipsUnion(rawCIdleClipsDict[component],rawCReworkClipsDict[component]))

for component in (Component):

    CIdleClipsDict[component]=removeClipsDuplicate(copy.deepcopy(rawCIdleClipsDict[component]), NNVATClips)
    CReworkClipsDict[component]=removeClipsDuplicate(copy.deepcopy(rawCReworkClipsDict[component]), NNVATClips)
    CReworkClipsDict[component]=removeClipsDuplicate(copy.deepcopy(CReworkClipsDict[component]), CIdleClipsDict[component])


drawCalculationComponent()


for component in (Component):
    CTaggedClipsDict[component]=CIdleClipsDict[component]+CReworkClipsDict[component]
    for i in range(len(CTaggedClipsDict[component])):
        if i<len(CIdleClipsDict[component]):
            CTaggedClipsDict[component][i].append("idle")
        else:
            CTaggedClipsDict[component][i].append("rework")
    CTaggedClipsDict[component].sort(key=lambda clip: clip[0])

step=0.02
actResults=[]

for i in range(ACT_NUM):

    componentNumReq=ACT_COMPONENT_NUM_REQUIREMENT_LIST[i]

    plt.axvline(x = actClipList[i][0], color = colormode255To1((251, 96, 127)), linewidth=0.2, linestyle='--')
    plt.axvline(x = actClipList[i][1], color = colormode255To1((251, 96, 127)), linewidth=0.2, linestyle='--')

    genIdleClips=[]
    genReworkClipsDict={
        1:[],
        1/2:[],
        1/3:[],
        2/3:[]
    }
    genInefficientClips=[]

    IWT=0
    IT=0
    RTPlusNTT=0

    actCTaggedClipsDict={}
    for component in (Component):      
        actCTaggedClipsDict[component]=getIntersection(copy.deepcopy(CTaggedClipsDict[component]),copy.deepcopy(actClipList[i]))

    actCClipIndexDict={}
    for component in (Component):
        actCClipIndexDict[component]=0
    
    for ts in np.arange(actClipList[i][0], actClipList[i][1], step):
        tags=[]
        for component in (Component):
            for clip in CTaggedClipsDict[component]:
                if clip[0]<=ts<clip[1]:
                    tags.append(clip[2])

        dur=step
        if(ts+dur>actClipList[i][1]):
            dur=actClipList[i][1]-ts

        if tags.count('idle')==3:
            IT+=dur
            if len(genIdleClips)==0 or genIdleClips[-1][1]+step<ts:
                genIdleClips.append([ts,ts+dur])
            else:
                genIdleClips[-1][1]=ts+dur
        elif tags.count('rework')!=0:
            kRework=tags.count('rework')/(3-tags.count('idle'))
            RTPlusNTT+=kRework*dur

            if len(genReworkClipsDict[kRework])==0 or genReworkClipsDict[kRework][-1][1]+dur<ts:
                genReworkClipsDict[kRework].append([ts,ts+dur])
            else:
                genReworkClipsDict[kRework][-1][1]=ts+dur
        elif tags.count('idle')!=0 and ACT_COMPONENT_NUM_REQUIREMENT_LIST[i]==2:
            kVT=(3-tags.count('idle'))/ACT_COMPONENT_NUM_REQUIREMENT_LIST[i]
            if kVT>1:
                kVT=1
            kIWT=1-kVT
            IWT+=kIWT*dur

            if kIWT!=0:
                if len(genInefficientClips)==0 or genInefficientClips[-1][1]+dur<ts:
                    genInefficientClips.append([ts,ts+dur])
                else:
                    genInefficientClips[-1][1]=ts+dur
    actResults.append(AnalyseResult(actClipList[i][1]-actClipList[i][0],getClipsTotDur(getIntersection(copy.deepcopy(NNVATClips),actClipList[i])),IT,RTPlusNTT,IWT,genIdleClips,genReworkClipsDict,genInefficientClips))
drawCalculationNNVAT()
drawCalculationIdle()
drawCalculationRework()
drawCalculationComponent()
drawCalculationActResult()
drawVSM()
if SVAct!=-1:
    actResults[SVAct].SV+=1



actResultDicts=[]
for i in range(ACT_NUM):
    actResultDicts.append(actResults[i].__dict__)

json_object = json.dumps(actResultDicts, indent=4)
 
# Writing to sample.json
with open(os.path.join(EXP_DIR_PATH, "actResult.json"), "w") as outfile:
    outfile.write(json_object)