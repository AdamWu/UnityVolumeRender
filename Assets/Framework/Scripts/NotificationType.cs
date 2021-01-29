using UnityEngine;

/// <summary>
///通知类型
/// </summary>
public class NotificationType {

	// UI
	public const string SHOW_ALERT = "show_alert";
	public const string SHOW_TIP = "show_tip";
	public const string HIDE_TIP = "hide_tip";

	// task
	public const string TASK_FINISH = "task_finish";										// 任务完成通知
	public const string TASK_STEP_FINISH = "task_step_finish";								// 任务中一小步完成
	public const string TASK_STEP_ERROR = "task_step_error";								// 错误操作
	public const string TASK_ENTITY_TRIGGER = "task_entity_trigger";						// entity触发
	public const string TASK_ENTITY_VARIABLE_CHANGE = "task_entity_variable_change";		// entity属性变化
	public const string TASK_RULE_WRONG = "task_rule_wrong";								// rule出错

    public const string TASK_ANSWER = "task_answer";                                        // 插题的答案
    // 例子
    public const string SHOW_MAIN_ENTITY = "show_main_entity";
	public const string HIDE_MAIN_ENTITY = "hide_main_entity";

	public const string ON_MOUSE_ENTER = "on_mouse_enter";
    public const string ON_MOUSE_EXIT = "on_mouse_exit";
    public const string ON_MOUSE_DOWN = "on_mouse_down";
    public const string ON_MOUSE_UP = "on_mouse_up";

    public const string ON_PLAYER_MOVE = "on_player_move";


    // volume render
    public const string VR_DATASET_LOAD = "vr_dataset_load";
    public const string VR_DATASET_SELECT = "vr_dataset_select";
    public const string VR_CONTROLPOINT_SELECT = "vr_controlpoint_select";
    public const string VR_CONTROLPOINT_ADD = "vr_controlpoint_add";
    public const string VR_CONTROLPOINT_DELETE = "vr_controlpoint_delete";
    public const string VR_CONTROLPOINT_UPDATE = "vr_controlpoint_update";
    public const string VR_RENDER_REFRESH = "vr_render_refresh";
    public const string VR_COLORMAP_UPDATE = "vr_colormap_update";

}