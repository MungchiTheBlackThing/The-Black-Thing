using System;
using UnityEngine;

[Serializable]
public class DotLocalizedText
{
    public string kor;
    public string eng;
}

[Serializable]
public class UIText
{
    public DotLocalizedText name_prompt;
    public DotLocalizedText name_recommendation;
    public DotLocalizedText name_confirm;

    public DotLocalizedText moon_device_comment;
    public DotLocalizedText moon_letter_title;

    public DotLocalizedText dot_first_meeting;
    public DotLocalizedText dot_waiting;
    public DotLocalizedText dot_okay;
    public DotLocalizedText dot_time_passing;

    public DotLocalizedText revisit_prompt;
    public DotLocalizedText revisit_button;
    public DotLocalizedText revisit_warning;

    public DotLocalizedText nav_thisway;
    public DotLocalizedText nav_herehere;

    public DotLocalizedText menu_main;
    public DotLocalizedText menu_letters;
    public DotLocalizedText menu_progress;
    public DotLocalizedText menu_settings;

    public DotLocalizedText today_square;
    public DotLocalizedText click_click_click;

    public DotLocalizedText dot_observation_end;
    public DotLocalizedText fly_snared;
    public DotLocalizedText dot_thinking_start;
    public DotLocalizedText fly_webbed;

    public DotLocalizedText story_special_note;
    public DotLocalizedText story_storage;
    public DotLocalizedText missed_chats_warning;
    public DotLocalizedText go_meet_dot;

    public DotLocalizedText room_polaroid;
    public DotLocalizedText nav_return;
    public DotLocalizedText menu_polaroids;

    public DotLocalizedText chat_colored;
    public DotLocalizedText chat_diary_trace;
    public DotLocalizedText chat_absent_log;

    public DotLocalizedText room_item_hint;
    public DotLocalizedText room_memory_fill;
    public DotLocalizedText skip_chat_warning;
    public DotLocalizedText skip_choice_reminder;
}
