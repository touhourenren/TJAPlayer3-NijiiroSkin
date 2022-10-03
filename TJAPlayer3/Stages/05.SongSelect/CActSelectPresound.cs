using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using FDK;
using System.Threading.Tasks;
using System.Threading;

namespace TJAPlayer3
{
	internal class CActSelectPresound : CActivity
	{
		// メソッド

		public CActSelectPresound()
		{
			base.b活性化してない = true;
		}
		public void tサウンド停止()
		{
			if( this.sound != null )
			{
				if(token != null)
				{
					token.Cancel();
				}
				this.sound.t再生を停止する();
				TJAPlayer3.Sound管理.tサウンドを破棄する( this.sound );
				this.sound = null;
			}
		}
		public void t選択曲が変更された()
		{
			Cスコア cスコア = TJAPlayer3.stage選曲.r現在選択中のスコア;
			
            if( ( cスコア != null ) && ( ( !( cスコア.ファイル情報.フォルダの絶対パス + cスコア.譜面情報.strBGMファイル名 ).Equals( this.str現在のファイル名 ) || ( this.sound == null ) ) || !this.sound.b再生中 ) )
			{
				this.tサウンド停止();
				this.tBGMフェードアウト開始();
                this.long再生位置 = -1;
				if( ( cスコア.譜面情報.strBGMファイル名 != null ) && ( cスコア.譜面情報.strBGMファイル名.Length > 0 ) )
				{
					//this.ct再生待ちウェイト = new CCounter( 0, CDTXMania.ConfigIni.n曲が選択されてからプレビュー音が鳴るまでのウェイトms, 1, CDTXMania.Timer );
                    if(TJAPlayer3.Sound管理.GetCurrentSoundDeviceType() != "DirectSound")
                    {
                        this.ct再生待ちウェイト = new CCounter(0, 1, 200, TJAPlayer3.Timer);
                    } else
                    {
                        this.ct再生待ちウェイト = new CCounter(0, 1, 200, TJAPlayer3.Timer);
                    }
                }
			}
		}


		// CActivity 実装

		public override void On活性化()
		{
			this.sound = null;
			this.str現在のファイル名 = "";
			this.ct再生待ちウェイト = null;
			this.ctPlaySoundフェードイン用 = new CCounter();
			this.ctPlaySoundフェードアウト用 = new CCounter();
            this.long再生位置 = -1;
            this.long再生開始時のシステム時刻 = -1;
			base.On活性化();
		}
		public override void On非活性化()
		{
			this.tサウンド停止();
			this.ct再生待ちウェイト = null;
			this.ctPlaySoundフェードアウト用 = null;
			this.ctPlaySoundフェードイン用 = null;
			base.On非活性化();
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				if( ( this.ctPlaySoundフェードイン用 != null ) && this.ctPlaySoundフェードイン用.b進行中 )
				{
					this.ctPlaySoundフェードイン用.t進行();

					if(this.sound != null)
						this.sound.AutomationLevel = this.ctPlaySoundフェードイン用.n現在の値;

					if( this.ctPlaySoundフェードイン用.b終了値に達した )
					{
						this.ctPlaySoundフェードイン用.t停止();
					}
				}
				if( ( this.ctPlaySoundフェードアウト用 != null ) && this.ctPlaySoundフェードアウト用.b進行中 )
				{
					this.ctPlaySoundフェードアウト用.t進行();
                    if (this.sound != null)
                        this.sound.AutomationLevel = CSound.MaximumAutomationLevel - this.ctPlaySoundフェードアウト用.n現在の値;
					if( this.ctPlaySoundフェードアウト用.b終了値に達した )
					{
						this.ctPlaySoundフェードアウト用.t停止();
					}
				}
				this.t進行処理_プレビューサウンド();

                if (this.sound != null)
                {
                    Cスコア cスコア = TJAPlayer3.stage選曲.r現在選択中のスコア;
                    if (long再生位置 == -1)
                    {
                        this.long再生開始時のシステム時刻 = CSound管理.rc演奏用タイマ.nシステム時刻ms;
                        this.long再生位置 = cスコア.譜面情報.nデモBGMオフセット;
                        this.sound.t再生位置を変更する(cスコア.譜面情報.nデモBGMオフセット);
                    }
                    else
                    {
                        this.long再生位置 = CSound管理.rc演奏用タイマ.nシステム時刻ms - this.long再生開始時のシステム時刻;
                    }
                    if (this.long再生位置 >= (this.sound.n総演奏時間ms - cスコア.譜面情報.nデモBGMオフセット) - 1 && this.long再生位置 <= (this.sound.n総演奏時間ms - cスコア.譜面情報.nデモBGMオフセット) + 0)
                        this.long再生位置 = -1;


                    //CDTXMania.act文字コンソール.tPrint( 0, 0, C文字コンソール.Eフォント種別.白, this.long再生位置.ToString() );
                    //CDTXMania.act文字コンソール.tPrint( 0, 20, C文字コンソール.Eフォント種別.白, (this.sound.n総演奏時間ms - cスコア.譜面情報.nデモBGMオフセット).ToString() );
                }
			}
			return 0;
		}


        // その他

        #region [ private ]
        //-----------------
        private CancellationTokenSource token; // 2019.03.23 kairera0467 マルチスレッドの中断処理を行うためのトークン
        private CCounter ctPlaySoundフェードイン用;
		private CCounter ctPlaySoundフェードアウト用;
		private CCounter ct再生待ちウェイト;
        private long long再生位置;
        private long long再生開始時のシステム時刻;
		private CSound sound;
		private string str現在のファイル名;
		
		private void tBGMフェードアウト開始()
		{
			if( this.ctPlaySoundフェードイン用 != null )
			{
				this.ctPlaySoundフェードイン用.t停止();
			}
			this.ctPlaySoundフェードアウト用 = new CCounter( 0, 100, 3, TJAPlayer3.Timer );
			this.ctPlaySoundフェードアウト用.n現在の値 = 0;
		}
		private void tBGMフェードイン開始()
		{
			if( this.ctPlaySoundフェードイン用 != null )
			{
				this.ctPlaySoundフェードアウト用.t停止();
			}
			this.ctPlaySoundフェードイン用 = new CCounter( 0, 100, 3, TJAPlayer3.Timer );
			this.ctPlaySoundフェードイン用.n現在の値 = 0;
		}
		private async void tプレビューサウンドの作成()
		{
			Cスコア cスコア = TJAPlayer3.stage選曲.r現在選択中のスコア;
			if( ( cスコア != null ) && !string.IsNullOrEmpty( cスコア.譜面情報.strBGMファイル名 ) && TJAPlayer3.stage選曲.eフェーズID != CStage.Eフェーズ.選曲_NowLoading画面へのフェードアウト )
			{
				string strPreviewFilename = cスコア.ファイル情報.フォルダの絶対パス + cスコア.譜面情報.Presound;
				try
                {
                    strPreviewFilename = cスコア.ファイル情報.フォルダの絶対パス + cスコア.譜面情報.strBGMファイル名;

					CSound tmps = await Task.Run<CSound>(() =>
					{
						token = new CancellationTokenSource();
						return TJAPlayer3.Sound管理.tサウンドを生成する(strPreviewFilename, ESoundGroup.SongPreview);
					});

					token.Token.ThrowIfCancellationRequested();
                    this.tサウンド停止();
                    this.sound = tmps;

                    // 2018-08-27 twopointzero - DO attempt to load (or queue scanning) loudness metadata here.
                    //                           Initialization, song enumeration, and/or interactions may have
                    //                           caused background scanning and the metadata may now be available.
                    //                           If is not yet available then we wish to queue scanning.
                    var loudnessMetadata = cスコア.譜面情報.SongLoudnessMetadata
                                           ?? LoudnessMetadataScanner.LoadForAudioPath(strPreviewFilename);

                    TJAPlayer3.SongGainController.Set( cスコア.譜面情報.SongVol, loudnessMetadata, this.sound );

                    this.sound.t再生を開始する( true );
                    if( long再生位置 == -1 )
                    {
                        this.long再生開始時のシステム時刻 = CSound管理.rc演奏用タイマ.nシステム時刻ms;
                        this.long再生位置 = cスコア.譜面情報.nデモBGMオフセット;
                        this.sound.t再生位置を変更する( cスコア.譜面情報.nデモBGMオフセット );
                        this.long再生位置 = CSound管理.rc演奏用タイマ.nシステム時刻ms - this.long再生開始時のシステム時刻;
                    }
                    //if( long再生位置 == this.sound.n総演奏時間ms - 10 )
                    //    this.long再生位置 = -1;

                    this.str現在のファイル名 = strPreviewFilename;
                    this.tBGMフェードイン開始();
                    Trace.TraceInformation( "プレビューサウンドを生成しました。({0})", strPreviewFilename );
                }
				catch (Exception e)
				{
					Trace.TraceError( e.ToString() );
					Trace.TraceError( "プレビューサウンドの生成に失敗しました。({0})", strPreviewFilename );
					if( this.sound != null )
					{
						this.sound.Dispose();
					}
					this.sound = null;
				}
			}
		}
		private void t進行処理_プレビューサウンド()
		{
			if( ( this.ct再生待ちウェイト != null ) && !this.ct再生待ちウェイト.b停止中 )
			{
				this.ct再生待ちウェイト.t進行();
				if (!this.ct再生待ちウェイト.b終了値に達してない)
				{
					this.ct再生待ちウェイト.t停止();
					if(!TJAPlayer3.stage選曲.bスクロール中)
						this.tプレビューサウンドの作成();
				}
			}
		}
		//-----------------
		#endregion
	}
}
