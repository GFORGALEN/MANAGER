建筑公司项目管理系统 AI 需求文档（MVP 版）
1. 项目概述

开发一个面向新西兰小型建筑公司的 Web 管理系统，支持老板、管理人员、普通工人通过网页端登录使用。系统需要同时适配桌面端和手机端，其中工人主要通过手机网页使用。

当前目标是完成一个可运行的 MVP，重点解决以下问题：

项目信息分散，缺少统一管理

工人无法方便地在手机上查看自己的任务

老板和管理层无法快速了解项目进度

施工任务、客户变更、现场附件缺少统一记录

不同角色看到的页面和能操作的功能需要区分

系统应基于角色权限控制，实现“不同用户登录后看到不同页面、拥有不同操作权限”。

2. 产品目标

本系统的核心目标是：

支持建筑公司内部不同角色登录和使用

支持项目、任务、变更、附件的完整基础管理

支持普通工人通过手机查看自己的任务并更新状态

支持老板查看整体情况并管理用户权限

支持管理人员创建项目、分配任务、维护资料

提供注册功能，允许新员工加入系统

注册后默认低权限，权限升级由老板或管理层控制

3. 用户角色定义

系统包含 3 类用户角色：

3.1 Owner（老板）

权限最高，负责全局查看、审批、用户管理。

主要职责：

查看所有项目

查看所有任务

查看所有变更

查看所有附件

创建、编辑、删除项目

查看所有用户

修改用户角色

启用/禁用用户

审批或驳回变更

查看系统整体概览

3.2 Manager（管理 / 项目经理）

负责项目执行与人员协调。

主要职责：

创建和编辑项目

查看项目详情

创建和编辑任务

给工人分配任务

更新任务状态

创建和提交变更

上传项目附件

查看自己负责的项目和任务

查看工人执行情况

3.3 Worker（普通工人）

权限最低，只处理自己被分配的工作。

主要职责：

登录系统

查看分配给自己的任务

查看任务详情

更新自己任务的状态

上传现场图片或附件

添加备注

查看自己参与的项目基本信息

修改自己的基础资料

4. 注册与登录规则
4.1 登录

用户使用邮箱或手机号 + 密码登录系统。

登录成功后：

返回 JWT Token

返回当前用户信息

返回用户角色

前端根据角色跳转到对应页面

角色跳转规则：

Owner → /owner/dashboard

Manager → /manager/dashboard

Worker → /worker/dashboard

4.2 注册

系统需要提供注册功能。

注册方式：

支持邮箱注册

手机号字段可选填，作为后续扩展

当前 MVP 阶段不做短信验证码

当前 MVP 阶段不做邮箱验证码

注册字段：

Name

Email

PhoneNumber（可选）

Password

ConfirmPassword

注册规则：

所有新注册用户默认角色为 Worker

不允许用户在注册时自行选择 Owner

不允许用户在注册时自行选择 Manager

Owner 账号由系统初始化创建

Manager 角色只能由 Owner 提升

Worker 可由 Owner 或 Manager 管理是否启用

5. 角色权限规则
5.1 项目 Project

Owner：查看 / 新建 / 编辑 / 删除全部项目

Manager：查看 / 新建 / 编辑项目，删除权限可保留给 Owner

Worker：只能查看自己参与项目的简要信息

5.2 任务 TaskItem

Owner：查看全部任务

Manager：查看 / 创建 / 编辑 / 分配 / 更新全部任务

Worker：只能查看自己的任务，只能更新自己的任务状态

5.3 变更 Variation

Owner：查看 / 审批 / 驳回

Manager：查看 / 创建 / 编辑 / 提交

Worker：默认不可见，或只读简要状态

5.4 附件 Attachment

Owner：查看全部

Manager：查看 / 上传 / 编辑 / 删除

Worker：查看与自己任务相关的附件，上传自己任务相关现场照片

5.5 用户 User

Owner：查看所有用户，修改角色，启用/禁用用户

Manager：查看自己团队用户，可管理 Worker

Worker：只能查看和编辑自己的基础资料

6. 系统核心模块

系统包含以下模块：

认证模块 Auth

用户模块 Users

项目模块 Projects

任务模块 Tasks

变更模块 Variations

附件模块 Attachments

我的任务模块 My Tasks

仪表盘模块 Dashboard

7. 页面需求
7.1 公共页面
LoginPage

功能：

输入邮箱/手机号和密码

登录

登录失败提示

登录成功后根据角色跳转

RegisterPage

功能：

输入姓名

输入邮箱

输入手机号（可选）

输入密码

确认密码

提交注册

注册成功后跳转登录页

提示“新账号默认作为 Worker，需要管理员分配更高权限”

7.2 Owner 页面
OwnerDashboardPage

展示：

项目总数

进行中项目数

待审批变更数量

今日待完成任务数

最近上传的附件

最近注册用户

UserManagementPage

功能：

查看用户列表

搜索用户

按角色筛选

修改用户角色

启用/禁用用户

查看用户详情

ProjectListPage

功能：

查看全部项目

搜索

分页

新建项目

编辑项目

删除项目

VariationApprovalPage

功能：

查看所有 Submitted 状态的变更

审批

驳回

填写审批备注

7.3 Manager 页面
ManagerDashboardPage

展示：

自己负责的项目数

今日待处理任务

待提交/待审批变更

最近附件上传记录

ProjectListPage

功能：

查看项目列表

搜索

分页

新建项目

编辑项目

查看详情

ProjectDetailPage

展示：

项目基本信息

项目任务

项目变更

项目附件

功能：

编辑项目

创建任务

创建变更

上传附件

TaskHubPage

功能：

查看任务列表

分页

筛选状态

按项目筛选

创建任务

编辑任务

分配任务给 Worker

更新任务状态

VariationHubPage

功能：

查看变更列表

分页

筛选状态

创建变更

编辑变更

提交变更

AttachmentHubPage

功能：

查看附件列表

上传文件

编辑附件元数据

删除附件

7.4 Worker 页面（手机优先）
WorkerDashboardPage

展示：

今日任务数量

进行中任务数量

已完成任务数量

快捷入口：我的任务 / 上传照片

页面风格要求：

卡片式布局

适配手机端

操作按钮大，方便点击

MyTasksPage

功能：

查看我的任务列表

按状态筛选：Todo / Doing / Done

按日期排序

点击进入任务详情

MyTaskDetailPage

展示：

任务标题

任务描述

所属项目

截止日期

当前状态

备注

附件列表

功能：

更新状态

上传现场照片

添加备注

MyProfilePage

功能：

查看个人资料

修改姓名

修改手机号

修改密码

退出登录

8. 数据模型需求
8.1 User

字段建议：

Id

Name

Email

PhoneNumber

PasswordHash

Role (Owner, Manager, Worker)

IsActive

CreatedAt

LastLoginAt

说明：

Email 唯一

PhoneNumber 暂不强制唯一，但可预留唯一约束扩展

Role 必填

IsActive 默认 true

8.2 Project

字段建议：

Id

Name

Description

ClientName

Address

Status

StartDate

EndDate

ManagerUserId

CreatedAt

关系：

一个 Project 对多个 TaskItem

一个 Project 对多个 Variation

一个 Project 对多个 Attachment

8.3 TaskItem

字段建议：

Id

ProjectId

Title

Description

Status (Todo, Doing, Done)

AssignedUserId

DueDate

Priority

Notes

CreatedAt

UpdatedAt

说明：

用 AssignedUserId 关联具体工人

Worker 只能访问 AssignedUserId = 自己的任务

8.4 Variation

字段建议：

Id

ProjectId

Title

Description

Status (Draft, Submitted, Approved, Rejected, NeedInfo)

RequestedByUserId

ApprovedByUserId

AmountImpact

CreatedAt

UpdatedAt

8.5 Attachment

字段建议：

Id

ProjectId

TaskItemId（可选）

FileName

FileUrl

ContentType

UploadedByUserId

Description

CreatedAt

说明：

附件既可以挂在项目下，也可以挂在任务下

TaskItemId 可为空

9. 后端 API 需求
9.1 Auth

需要实现：

POST /api/auth/login

POST /api/auth/register

GET /api/auth/me

要求：

登录成功返回 token + user info + role

注册成功默认 Worker

GET /api/auth/me 返回当前登录用户信息

9.2 Users

需要实现：

GET /api/users

GET /api/users/{id}

PUT /api/users/{id}

PATCH /api/users/{id}/role

PATCH /api/users/{id}/active

权限要求：

Owner 可访问所有

Manager 可访问部分 Worker 数据

Worker 只能访问自己

9.3 Projects

需要实现：

GET /api/projects

GET /api/projects/{id}

POST /api/projects

PUT /api/projects/{id}

DELETE /api/projects/{id}

要求：

支持分页

支持搜索

支持按状态筛选

9.4 Tasks

需要实现：

GET /api/projects/{projectId}/tasks

GET /api/tasks/{id}

POST /api/projects/{projectId}/tasks

PUT /api/tasks/{id}

PATCH /api/tasks/{id}/status

新增建议接口：

GET /api/my/tasks

GET /api/my/tasks/{id}

PATCH /api/my/tasks/{id}/status

要求：

Manager 可管理所有项目任务

Worker 只能访问自己的任务

9.5 Variations

需要实现：

GET /api/projects/{projectId}/variations

GET /api/variations/{id}

POST /api/projects/{projectId}/variations

PUT /api/variations/{id}

PATCH /api/variations/{id}/status

要求：

Manager 可创建和提交

Owner 可审批和驳回

9.6 Attachments

需要实现：

GET /api/projects/{projectId}/attachments

GET /api/attachments/{id}

POST /api/projects/{projectId}/attachments

PUT /api/attachments/{id}

DELETE /api/attachments/{id}

POST /api/projects/{projectId}/attachments/upload

新增建议接口：

POST /api/my/tasks/{id}/attachments/upload

10. 前端需求

技术栈：

Vue 3

TypeScript

Ant Design Vue

Axios

Vue Router

要求：

保留当前后台布局

新增角色分流

新增手机端友好页面

登录后根据角色自动跳转

前端通过 JWT 调用接口

所有请求自动注入 Bearer Token

路由守卫拦截未登录用户

路由守卫校验角色页面访问权限

11. 路由设计建议
公共路由

/login

/register

Owner 路由

/owner/dashboard

/owner/projects

/owner/projects/:id

/owner/variations

/owner/users

Manager 路由

/manager/dashboard

/manager/projects

/manager/projects/:id

/manager/tasks

/manager/variations

/manager/attachments

Worker 路由

/worker/dashboard

/worker/tasks

/worker/tasks/:id

/worker/profile

12. UI/UX 要求
桌面端

Owner 和 Manager 页面可继续使用后台管理风格

左侧菜单导航

表格 + 分页 + 筛选

手机端

Worker 页面优先适配手机

使用卡片式布局

减少复杂表格

关键按钮大且明显

页面层级简洁

减少需要输入的大段文字

13. 权限控制实现要求

后端必须做真实权限校验，不能只靠前端隐藏按钮。

要求：

JWT 中包含用户 ID 和角色

每个接口根据角色校验权限

Worker 访问任务时必须校验 AssignedUserId == 当前用户 ID

Worker 不能修改他人任务

Worker 不能访问用户管理接口

Owner 才能修改角色为 Manager 或 Owner

注册接口默认创建 Worker

前端要求：

根据角色决定菜单显示

根据角色决定跳转首页

无权限页面自动重定向

14. 日志与异常处理要求

后端继续保留：

全局异常处理中间件

基础结构化日志

新增要求：

记录登录成功/失败

记录注册

记录角色变更

记录任务状态变更

记录变更审批

记录附件上传

15. 验收标准

当以下场景能够跑通时，视为本阶段完成：

场景 1：注册与登录

新用户可以通过注册页面创建账号

注册后默认角色为 Worker

用户可以登录

登录后根据角色进入不同页面

场景 2：Owner 管理用户

Owner 可以查看用户列表

Owner 可以把某个 Worker 提升为 Manager

Owner 可以禁用某个用户

场景 3：Manager 管理项目和任务

Manager 可以创建项目

Manager 可以创建任务

Manager 可以把任务分配给某个 Worker

Manager 可以查看任务进度

场景 4：Worker 使用手机完成工作

Worker 登录后进入手机友好的 dashboard

Worker 可以查看自己的任务列表

Worker 可以打开任务详情

Worker 可以把任务从 Todo 改成 Doing 或 Done

Worker 可以上传现场图片

场景 5：变更审批

Manager 可以创建变更并提交

Owner 可以审批或驳回变更

16. 开发优先级
Phase 1：必须先完成

POST /api/auth/register

GET /api/auth/me

User 角色与启用状态

TaskItem 的 AssignedUserId

GET /api/my/tasks

登录后按角色跳转

Worker 移动端页面

Phase 2：紧接着完成

UserManagementPage

角色修改

启用/禁用用户

Variation 审批流

Worker 上传任务图片

Phase 3：后续增强

Dashboard 统计图

邀请码注册

修改密码

邮件通知

短信登录

AI 自动生成日报/周报/变更摘要

17. 给 AI 编码助手的实现要求

请严格按以下原则实现：

基于现有项目继续开发，不要推翻当前结构

保留现有 ASP.NET Core Web API + EF Core + SQL Server 后端架构

保留现有 Vue 3 + TypeScript + Ant Design Vue 前端架构

优先复用当前实体和控制器

新增代码应与当前命名风格一致

所有新增接口都要加 Swagger 注释

所有新增页面都要接入现有 router 和 axios 实例

所有权限逻辑必须后端校验

所有页面优先保证可运行，再考虑美化

Worker 页面优先移动端体验

18. 一句话产品定义

这是一个 面向小型建筑公司的、支持老板/管理/工人三类角色、可在桌面端和手机端使用的项目管理与施工协作系统 MVP