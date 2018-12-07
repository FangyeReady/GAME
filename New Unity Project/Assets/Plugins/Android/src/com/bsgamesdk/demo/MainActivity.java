package com.bsgamesdk.demo;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.bsgamesdk.android.BSGameSdk;
import com.bsgamesdk.android.callbacklistener.AccountCallBackListener;
import com.bsgamesdk.android.callbacklistener.BSGameSdkError;
import com.bsgamesdk.android.callbacklistener.CallbackListener;
import com.bsgamesdk.android.callbacklistener.ExitCallbackListener;
import com.bsgamesdk.android.callbacklistener.InitCallbackListener;
import com.bsgamesdk.android.callbacklistener.OrderCallbackListener;
import com.bsgamesdk.android.utils.LogUtils;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.os.SystemClock;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity {

	private Activity mContext;

	private Button btnPay;
	private Button btnReg;
	private Button btnLogin;
	private Button btnLogout;
	private Button btnCheckLogin;
	private Button btnGetUserInfo;
	private Button btnNotifyZone;
	private Button btnCreateRole;
	private Button btnCheckRealNameAuth;
	private Button btnExit;
	
	public static final int OK = 1;
	// 用于存储用户信息
	private SharedPreferences preferences;
	// 声明BSGameSdk实例
	private BSGameSdk gameSdk;
	// 商户参数
	private String merchant_id;
	private String app_id;
	private String server_id;
	private String app_key;

	private Handler mHandler = new Handler() {
		@Override
		public void handleMessage(Message msg) {
			super.handleMessage(msg);

			switch (msg.what) {
			case MainActivity.OK:
				Toast.makeText(mContext, (String) msg.obj, Toast.LENGTH_SHORT)
						.show();
				break;
			}
		}
	};

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		mContext = MainActivity.this;
		// 配置参数
		merchant_id = "2";
		app_id = "2";
		server_id = "2";
		app_key = "202cb962ac59075b964b07152d234b70";

		// 获得BSGameSdk实例

		BSGameSdk.initialize(true, mContext, merchant_id, app_id,
				server_id, app_key, new InitCallbackListener() {
                    @Override
                    public void onSuccess() {
                        makeToast("initialize onSuccess" );
                    }

                    @Override
                    public void onFailed() {
                        makeToast("initialize onFailed" );
                    }
                },new ExitCallbackListener() {
					
					@Override
					public void onExit() {
						finish();
						System.exit(0);
					}
				});
		
		setContentView(R.layout.activity_main);

		gameSdk = BSGameSdk.getInstance();
		
		gameSdk.setAccountListener(new AccountCallBackListener() {
			
			@Override
			public void onAccountInvalid() {
				makeToast("账号已登出");
			}
		});
		
		preferences = mContext.getSharedPreferences("demouser",
				Context.MODE_PRIVATE);

		TextView title = (TextView) findViewById(R.id.sdkTitleView);
		title.setText("BSGameSdk_Demo_" + gameSdk.sdkVersion());
		
		btnPay = (Button) findViewById(R.id.buttonPay);
		btnReg = (Button) findViewById(R.id.buttonBSGameSdkReg);
		btnLogin = (Button) findViewById(R.id.buttonBSGameSdkLogin);
		btnCheckLogin = (Button) findViewById(R.id.buttonCheckLogin);
		btnGetUserInfo = (Button) findViewById(R.id.buttonGetUserInfo);
		btnLogout = (Button) findViewById(R.id.buttonBSGameSdkLogin2);
		btnNotifyZone = (Button) findViewById(R.id.buttonNotifyZone);
		btnCreateRole = (Button) findViewById(R.id.buttonCreateRole);
		btnCheckRealNameAuth = (Button) findViewById(R.id.buttonCheckRealNameAuth);
		btnExit= (Button) findViewById(R.id.buttonExit);
		
		btnExit.setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View v) {
				gameSdk.exit(new ExitCallbackListener() {
					
					@Override
					public void onExit() {
						makeToast("退出游戏成功!");
						finish();
						System.exit(0);
					}
				});

			}
		});
		
		btnPay.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				showDialog(mContext);
			}
		});

		btnReg.setOnClickListener(new OnClickListener() {

			// 注册操作
			public void onClick(View v) {
				System.out.println("button click");

				gameSdk.register(new CallbackListener() {

					@Override
					public void onSuccess(Bundle arg0) {
						// 此处为操作成功时执行，返回值通过Bundle传回
						LogUtils.d("onSuccess");
						// 注册成功后已退出登录，清除保存的信息
						preferences.edit().clear().commit();
						String result = arg0.getString("result");
						makeToast("return: " + result);
					}

					@Override
					public void onFailed(BSGameSdkError arg0) {
						// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
					}

					@Override
					public void onError(BSGameSdkError arg0) {
						// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onError\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onError\nErrorCode : " + arg0.getErrorCode()
								+ "\nErrorMessage : " + arg0.getErrorMessage());
					}

				});
			}
		});

		btnLogin.setOnClickListener(new OnClickListener() {

			// 登录操作
			public void onClick(View v) {
					System.out.println("button click");

					gameSdk.login(new CallbackListener() {

						@Override
						public void onSuccess(Bundle arg0) {
							// 此处为操作成功时执行，返回值通过Bundle传回
							
							LogUtils.d("onSuccess");
							String uid = arg0.getString("uid");
							String userName = arg0.getString("username");
							String access_token = arg0.getString("access_token");
							String expire_times = arg0.getString("expire_times");
							String refresh_token = arg0.getString("refresh_token");
							String nickname = arg0.getString("nickname");
							preferences.edit().clear().commit();
							preferences.edit().putString("username", userName)
									.commit();
							preferences.edit().putString("uid", uid).commit();

							makeToast("uid: " + uid + " nickname: " + nickname
									+ " access_token: " + access_token
									+ " expire_times: " + expire_times
									+ " refresh_token: " + refresh_token);
						}

						@Override
						public void onFailed(BSGameSdkError arg0) {
							// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
							LogUtils.d("onFailed\nErrorCode : "
									+ arg0.getErrorCode() + "\nErrorMessage : "
									+ arg0.getErrorMessage());
							makeToast("onFailed\nErrorCode : "
									+ arg0.getErrorCode() + "\nErrorMessage : "
									+ arg0.getErrorMessage());
						}

						@Override
						public void onError(BSGameSdkError arg0) {
							// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
							LogUtils.d("onError\nErrorCode : "
									+ arg0.getErrorCode() + "\nErrorMessage : "
									+ arg0.getErrorMessage());
							makeToast("onError\nErrorCode : " + arg0.getErrorCode()
									+ "\nErrorMessage : " + arg0.getErrorMessage());
						}
					});
			}
		});

		btnCheckLogin.setOnClickListener(new OnClickListener() {

			// 判断用户是否登录
			public void onClick(View v) {
				System.out.println("button click");

				gameSdk.isLogin(new CallbackListener() {

					@Override
					public void onSuccess(Bundle arg0) {
						// 此处为操作成功时执行，返回值通过Bundle传回
						LogUtils.d("onSuccess");
						boolean logined = arg0.getBoolean("logined", false);
						makeToast("logined: " + logined);
					}

					@Override
					public void onFailed(BSGameSdkError arg0) {
						// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
					}

					@Override
					public void onError(BSGameSdkError arg0) {
						// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onError\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onError\nErrorCode : " + arg0.getErrorCode()
								+ "\nErrorMessage : " + arg0.getErrorMessage());
					}
				});
			}
		});

		btnGetUserInfo.setOnClickListener(new OnClickListener() {

			// 获取用户信息
			public void onClick(View v) {
				System.out.println("button click");

				gameSdk.getUserInfo(new CallbackListener() {

					@Override
					public void onSuccess(Bundle arg0) {
						// 此处为操作成功时执行，返回值通过Bundle传回
						String uid = arg0.getString("uid");
						String username = arg0.getString("username");
						String access_token = arg0.getString("access_token");
						String expire_times = arg0.getString("expire_times");
						String refresh_token = arg0.getString("refresh_token");
						String avatar = arg0.getString("avatar");
						String s_avatar = arg0.getString("s_avatar");

						String lastLoginTime = arg0
								.getString("last_login_time");
						LogUtils.d("onSuccess\nuid: " + uid + " username: "
								+ username + " access_token: " + access_token
								+ " expire_times: " + expire_times
								+ " refresh_token: " + refresh_token
								+ " lastLoginTime: " + lastLoginTime
								+ " avatar " + avatar + " s_avatar " + s_avatar);
						makeToast("uid: " + uid + " username: " + username
								+ " access_token: " + access_token
								+ " expire_times: " + expire_times
								+ " refresh_token: " + refresh_token
								+ " lastLoginTime: " + lastLoginTime
								+ " avatar " + avatar + " s_avatar " + s_avatar);
					}

					@Override
					public void onFailed(BSGameSdkError arg0) {
						// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
					}

					@Override
					public void onError(BSGameSdkError arg0) {
						// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onError\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onError\nErrorCode : " + arg0.getErrorCode()
								+ "\nErrorMessage : " + arg0.getErrorMessage());
					}
				});
			}
		});
		
		btnNotifyZone.setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View v) {
				gameSdk.notifyZone(server_id, "bilibili-1区", "1", "路人甲");
			}
		});

		btnLogout.setOnClickListener(new OnClickListener() {

			// 用户注销操作
			public void onClick(View v) {

				gameSdk.logout(new CallbackListener() {

					@Override
					public void onSuccess(Bundle arg0) {
						// 此处为操作成功时执行，返回值通过Bundle传回
						LogUtils.d("onSuccess");
						String tips = arg0.getString("tips");
						preferences.edit().clear().commit();
						makeToast("tips: " + tips);
					}

					@Override
					public void onFailed(BSGameSdkError arg0) {
						// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onFailed\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
					}

					@Override
					public void onError(BSGameSdkError arg0) {
						// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onError\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onError\nErrorCode : " + arg0.getErrorCode()
								+ "\nErrorMessage : " + arg0.getErrorMessage());
					}
				});
			}
		});
		
		final String role = "路人甲"; //角色名
		final String role_id = "123456"; //角色id
 		
		btnCreateRole.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {			
				//创建角色时候成功调用
				gameSdk.createRole(role, role_id);
				
			}
		});
		
		btnCheckRealNameAuth.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {

				gameSdk.isRealNameAuth(new CallbackListener() {
					@Override
					public void onSuccess(Bundle bundle) {
						LogUtils.d("onSuccess");
						boolean isRealNameAuth = bundle.getBoolean("isRealNameAuth", false);
						makeToast("isRealNameAuth: " + isRealNameAuth);
					}

					@Override
					public void onFailed(BSGameSdkError error) {
						// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onFailed\nErrorCode : "
								+ error.getErrorCode() + "\nErrorMessage : "
								+ error.getErrorMessage());
						makeToast("onFailed\nErrorCode : "
								+ error.getErrorCode() + "\nErrorMessage : "
								+ error.getErrorMessage());
					}

					@Override
					public void onError(BSGameSdkError error) {
						// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onError\nErrorCode : "
								+ error.getErrorCode() + "\nErrorMessage : "
								+ error.getErrorMessage());
						makeToast("onError\nErrorCode : " + error.getErrorCode()
								+ "\nErrorMessage : " + error.getErrorMessage());
					}
				});
			}
		});
	}

	// 显示基于Layout的AlertDialog
	private void showDialog(Context context) {
		LayoutInflater inflater = LayoutInflater.from(this);
		final View textEntryView = inflater.inflate(R.layout.mydialog, null);
		final EditText edtInput = (EditText) textEntryView
				.findViewById(R.id.edit_fee);
		final Dialog dialog = new Dialog(context);
		textEntryView.findViewById(R.id.bsgamesdk_pay_confirm).setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				String fee = edtInput.getText().toString();
				if (null == fee || fee.equals("")) {
					makeToast("信息格式有误！");
				} else if ("".equals(preferences.getString("username", ""))) {
					makeToast("您还未登录！");
					dialog.dismiss();
				} else {
					doTest(Integer.valueOf(fee));
					dialog.dismiss();
				}
			}
		});
		textEntryView.findViewById(R.id.bsgamesdk_pay_cancel).setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				dialog.dismiss();
			}
		});
		dialog.setCancelable(false);
		dialog.requestWindowFeature(Window.FEATURE_NO_TITLE);
		dialog.setContentView(textEntryView);
		Window dialogWindow = dialog.getWindow();
		WindowManager.LayoutParams lp = dialogWindow.getAttributes();
		dialogWindow.setGravity(Gravity.CENTER);
		final float scale = context.getResources().getDisplayMetrics().density;
		lp.width =(int) (300 * scale + 0.5f); // 宽度300dp
		dialogWindow.setAttributes(lp);
		dialog.show();
	}
	
	private void doTest(int fee) {
		String userName = preferences.getString("username", "test");
		String payUid = preferences.getString("uid", "88");
		String nickname = preferences.getString("nickname", "默认昵称");
		
		/**
		 * 此部分为支付参数签名
		 *
		 * *** 重要说明***
		 *
		 * 生成签名的过程需要在您的服务端完成，客户端这里只是给出一个实例，详情参照服务端文档
		 */
		String notify_url = "";//不为空的话支付后异步通知到此地址，否则异步通知到正式地址，此字段可用于沙盒支付，正式上线前请置空
		String out_trade_number = String.valueOf(System.currentTimeMillis());
		int gameMoney = (int) ((fee / 100.0) * 10);

		String data = String.valueOf(gameMoney) + String.valueOf(fee) + notify_url + out_trade_number;
		//秘钥为服务端secretKey
		String order_sign = sign(data, "wgfykwenagycuf92zzuragjm7mxhvytt");
		
		int uid = Integer.valueOf(payUid);
		// 支付操作
		gameSdk.pay(uid, userName, nickname, server_id, fee,
		/* 注意这里fee是以分为单位的，以元为单位换算时要先除以100.0 */(int) ((fee / 100.0) * 10),
				out_trade_number, "subject", "body",
				"test for new parameters", notify_url, order_sign, new OrderCallbackListener() {
					@Override
					public void onSuccess(String out_trade_no, String bs_trade_no) {
						// 此处为操作成功时执行，返回值通过Bundle传回
						LogUtils.d("onSuccess");
						makeToast("CPTradeNo: " + out_trade_no +
								  "\nBSTradeNo: " + bs_trade_no );
					}

					@Override
					public void onFailed(String out_trade_no,
							BSGameSdkError arg0) {
						// 此处为操作失败时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onFailed\n" + "payOutTradeNo: "
								+ out_trade_no + "\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onFailed\n" + "payOutTradeNo: "
								+ out_trade_no + "\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
					}

					@Override
					public void onError(String out_trade_no, BSGameSdkError arg0) {
						// 此处为操作异常时执行，返回值为BSGameSdkError类型变量，其中包含ErrorCode和ErrorMessage
						LogUtils.d("onError\n" + "payOutTradeNo: "
								+ out_trade_no + "\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
						makeToast("onError\n" + "payOutTradeNo: "
								+ out_trade_no + "\nErrorCode : "
								+ arg0.getErrorCode() + "\nErrorMessage : "
								+ arg0.getErrorMessage());
					}
				});

	}

	private void makeToast(String result) {
		// TODO Auto-generated method stub
		Message msg = new Message();
		msg.what = MainActivity.OK;
		msg.obj = result;
		mHandler.sendMessage(msg);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	
	public String sign(String data, String secretKey) {
		return MD5.sign(data, secretKey);
	}
}
