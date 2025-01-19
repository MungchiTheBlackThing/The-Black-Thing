public enum DotAnimState
{
    anim_default = 0, //1
    anim_bed, //2
    anim_reading, //3
    anim_writing, //4
    anim_mold, //5
    anim_bounce, //6
    anim_laptop, //7
    anim_walking, //8
    anim_mold2, //9
    anim_happy, //10
    anim_spiderweb1, //11
    anim_spiderweb2, //12
    anim_eyesclosed,//13
    anim_eyescorner,//14
    anim_eyesdown,//15
    anim_eyesside,//16
    anim_eyesup,//17
    anim_sleepy_bed,//18
    anim_sleepy_spiderweb, //19
    anim_mud, //DotAnimState 20 - Chapter 
    anim_eyeswide, /*Idle 20*/

    anim_eyesblink, //21
    anim_eyesclosed_turn, //22
    anim_eyescorner_turn, //23
    anim_eyesdown_turn, //24
    anim_eyesside_turn, //25
    anim_eyesup_turn, //26
    anim_sub_wish, //27
    anim_sub_birdanddot, //28
    anim_sub_birdnest, //29
    anim_sub_caterpillar1, //30
    anim_sub_caterpillar2, //31

    anim_sub_deadbug1, //32
    anim_sub_deadbug2, //33
    anim_sub_dottoflamingo, //34
    anim_sub_flamingotodot, //35
    anim_sub_flamingo, //36
    anim_sub_dreamcatcher1, //37
    anim_sub_dreamcatcher2, //38
    anim_sub_roundthings1, //39
    anim_sub_roundthings2, //40
    anim_sub_spiderdot, //41
    anim_sub_raggedyann1, //42
    anim_sub_raggedyann2, //43
    anim_sub_raggedyann3, //44
    anim_sub_hands, //45
    anim_sub_heart, //46
    anim_sub_hurt1, //47
    anim_sub_hurt2, //48
    anim_move = 50, //50
    anim_idle, //51
    anim_sub_ch_heart, //52
    anim_sub_light, /*Sub 53*/

    anim_diary,
    anim_sleep, /*Trigger*/
    anim_watching,  /*Phase*/

    anim_trigger_play, /*Trigger*/

    body_default1, /*Main Body 58*/
    body_default2,
    body_default2_turn,
    body_bounce,
    body_move,
    body_rhythm,
    body_spikey,
    body_spikey_turn,
    body_hmm,
    body_hmm_turn,
    body_mud,
    body_mud_turn,
    body_trembling,
    body_ch2tremble,
    body_ch2drop,
    body_draw,
    body_draw2,
    body_cup,
    body_cup2,
    body_spiderweb_move,
    body_mudtodefault,
    body_ch_heart,
    body_ch_heartdown,
    body_ch_heartup,
    body_black_heart_1,
    body_black_heart_2,
    body_black_heart_3,
    //¸ùÅü

    phase_sleep, /*Trigger*/
}

public enum DotPatternState
{
    Default = 1,
    Sub = 2,
    Main = 3,
    Phase = 4,
    Trigger = 5,
    Tutorial = 6
}

public enum DotEyes
{
    face_null = 0,
    face_blink = 1,
    face_noblink = 2,
    face_squeeze = 3,
    face_close_turn,
    face_close,
    face_open,
    face_eyeroll,
    face_eyeroll2,
    face_eyeroll3,
    face_mad,
    face_staryeyes,
    face_thinking,
    face_omg,
    face_sad,
    face_draw,
    face_eyeroll3_stay
};
