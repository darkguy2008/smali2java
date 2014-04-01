.class public Lcom/netqin/NqSmsFilter;
.super Ljava/lang/Object;
.source "NqSmsFilter.java"


# static fields
.field private static final CONTENT_URI:Landroid/net/Uri;

.field private static filter:Lcom/netqin/NqSmsFilter;


# instance fields
.field private ctx:Landroid/content/Context;


# direct methods
.method static constructor <clinit>()V
    .registers 1

    .prologue
    .line 23
    const/4 v0, 0x0

    sput-object v0, Lcom/netqin/NqSmsFilter;->filter:Lcom/netqin/NqSmsFilter;

    .line 28
    const-string v0, "content://com.netqin.provider.SmsFilterProvider"

    invoke-static {v0}, Landroid/net/Uri;->parse(Ljava/lang/String;)Landroid/net/Uri;

    move-result-object v0

    sput-object v0, Lcom/netqin/NqSmsFilter;->CONTENT_URI:Landroid/net/Uri;

    .line 22
    return-void
.end method

.method private constructor <init>(Landroid/content/Context;)V
    .registers 2
    .param p1, "ctx"    # Landroid/content/Context;

    .prologue
    .line 25
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 26
    iput-object p1, p0, Lcom/netqin/NqSmsFilter;->ctx:Landroid/content/Context;

    .line 27
    return-void
.end method

.method public static declared-synchronized getInstance(Landroid/content/Context;)Lcom/netqin/NqSmsFilter;
    .registers 3
    .param p0, "context"    # Landroid/content/Context;

    .prologue
    .line 36
    const-class v1, Lcom/netqin/NqSmsFilter;

    monitor-enter v1

    :try_start_3
    sget-object v0, Lcom/netqin/NqSmsFilter;->filter:Lcom/netqin/NqSmsFilter;

    if-nez v0, :cond_e

    .line 37
    new-instance v0, Lcom/netqin/NqSmsFilter;

    invoke-direct {v0, p0}, Lcom/netqin/NqSmsFilter;-><init>(Landroid/content/Context;)V

    sput-object v0, Lcom/netqin/NqSmsFilter;->filter:Lcom/netqin/NqSmsFilter;

    .line 40
    :cond_e
    sget-object v0, Lcom/netqin/NqSmsFilter;->filter:Lcom/netqin/NqSmsFilter;
    :try_end_10
    .catchall {:try_start_3 .. :try_end_10} :catchall_12

    monitor-exit v1

    return-object v0

    .line 36
    :catchall_12
    move-exception v0

    monitor-exit v1

    throw v0
.end method


# virtual methods
.method public nqGetNetQinLogo()Landroid/graphics/Bitmap;
    .registers 5

    .prologue
    .line 94
    const/4 v0, 0x0

    .line 96
    .local v0, "b":Landroid/graphics/Bitmap;
    :try_start_1
    iget-object v2, p0, Lcom/netqin/NqSmsFilter;->ctx:Landroid/content/Context;

    invoke-virtual {v2}, Landroid/content/Context;->getResources()Landroid/content/res/Resources;

    move-result-object v2

    invoke-virtual {v2}, Landroid/content/res/Resources;->getAssets()Landroid/content/res/AssetManager;

    move-result-object v2

    const-string/jumbo v3, "netqin_icon.png"

    invoke-virtual {v2, v3}, Landroid/content/res/AssetManager;->open(Ljava/lang/String;)Ljava/io/InputStream;

    move-result-object v2

    invoke-static {v2}, Landroid/graphics/BitmapFactory;->decodeStream(Ljava/io/InputStream;)Landroid/graphics/Bitmap;
    :try_end_15
    .catch Ljava/io/IOException; {:try_start_1 .. :try_end_15} :catch_17

    move-result-object v0

    .line 103
    :goto_16
    return-object v0

    .line 98
    :catch_17
    move-exception v1

    .line 100
    .local v1, "e":Ljava/io/IOException;
    invoke-virtual {v1}, Ljava/io/IOException;->printStackTrace()V

    goto :goto_16
.end method

.method public nqGetNetQinText()Ljava/lang/String;
    .registers 9

    .prologue
    const/4 v2, 0x0

    .line 77
    const-string v7, "Powered by NetQin"

    .line 78
    .local v7, "s":Ljava/lang/String;
    iget-object v1, p0, Lcom/netqin/NqSmsFilter;->ctx:Landroid/content/Context;

    invoke-virtual {v1}, Landroid/content/Context;->getContentResolver()Landroid/content/ContentResolver;

    move-result-object v0

    .line 79
    .local v0, "cr":Landroid/content/ContentResolver;
    sget-object v1, Lcom/netqin/NqSmsFilter;->CONTENT_URI:Landroid/net/Uri;

    const-string v3, "NetqinText"

    move-object v4, v2

    move-object v5, v2

    invoke-virtual/range {v0 .. v5}, Landroid/content/ContentResolver;->query(Landroid/net/Uri;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;)Landroid/database/Cursor;

    move-result-object v6

    .line 80
    .local v6, "c":Landroid/database/Cursor;
    if-eqz v6, :cond_23

    .line 81
    invoke-interface {v6}, Landroid/database/Cursor;->moveToFirst()Z

    move-result v1

    if-eqz v1, :cond_20

    .line 82
    const/4 v1, 0x0

    invoke-interface {v6, v1}, Landroid/database/Cursor;->getString(I)Ljava/lang/String;

    move-result-object v7

    .line 84
    :cond_20
    invoke-interface {v6}, Landroid/database/Cursor;->close()V

    .line 86
    :cond_23
    return-object v7
.end method

.method public nqSmsFilter(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Z
    .registers 15
    .param p1, "smsNumber"    # Ljava/lang/String;
    .param p2, "smsContent"    # Ljava/lang/String;
    .param p3, "packageName"    # Ljava/lang/String;
    .param p4, "appName"    # Ljava/lang/String;

    .prologue
    const/4 v2, 0x0

    const/4 v8, 0x1

    const/4 v9, 0x0

    .line 52
    const/4 v6, 0x0

    .line 54
    .local v6, "b":I
    const/4 v1, 0x4

    new-array v4, v1, [Ljava/lang/String;

    aput-object p1, v4, v9

    aput-object p2, v4, v8

    const/4 v1, 0x2

    aput-object p3, v4, v1

    const/4 v1, 0x3

    aput-object p4, v4, v1

    .line 56
    .local v4, "args":[Ljava/lang/String;
    iget-object v1, p0, Lcom/netqin/NqSmsFilter;->ctx:Landroid/content/Context;

    invoke-virtual {v1}, Landroid/content/Context;->getContentResolver()Landroid/content/ContentResolver;

    move-result-object v0

    .line 57
    .local v0, "cr":Landroid/content/ContentResolver;
    sget-object v1, Lcom/netqin/NqSmsFilter;->CONTENT_URI:Landroid/net/Uri;

    move-object v3, v2

    move-object v5, v2

    invoke-virtual/range {v0 .. v5}, Landroid/content/ContentResolver;->query(Landroid/net/Uri;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;)Landroid/database/Cursor;

    move-result-object v7

    .line 58
    .local v7, "c":Landroid/database/Cursor;
    if-eqz v7, :cond_2e

    .line 59
    invoke-interface {v7}, Landroid/database/Cursor;->moveToFirst()Z

    move-result v1

    if-eqz v1, :cond_2b

    .line 60
    invoke-interface {v7, v9}, Landroid/database/Cursor;->getInt(I)I

    move-result v6

    .line 62
    :cond_2b
    invoke-interface {v7}, Landroid/database/Cursor;->close()V

    .line 65
    :cond_2e
    if-ne v6, v8, :cond_32

    move v1, v8

    .line 68
    :goto_31
    return v1

    :cond_32
    move v1, v9

    goto :goto_31
.end method
