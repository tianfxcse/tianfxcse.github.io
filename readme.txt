<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
    <title>流程任务管理面板 - 任务条件与分配模拟</title>
    <!-- 引入 jQuery, Vue.js, 以及基础样式 -->
    <script src="https://code.jquery.com/jquery-3.7.1.min.js" integrity="sha256-/JqT3SQfawRcv/BIHPThkBvs0OEvtFFmqPF/lYI/Cxo=" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/vue@2.7.16/dist/vue.js"></script>
    <!-- 使用清爽的字体和现代风格 -->
    <style>
        * {
            box-sizing: border-box;
            font-family: 'Segoe UI', Roboto, 'Helvetica Neue', sans-serif;
        }
        body {
            background: #f0f2f5;
            margin: 0;
            padding: 24px;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }
        .app-container {
            max-width: 1400px;
            width: 100%;
            background: white;
            border-radius: 28px;
            box-shadow: 0 12px 30px rgba(0,0,0,0.08);
            overflow: hidden;
            padding: 24px 28px 36px 28px;
            transition: all 0.2s;
        }
        h1 {
            font-size: 1.8rem;
            font-weight: 600;
            margin: 0 0 8px 0;
            color: #1e293b;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .sub {
            color: #475569;
            border-left: 4px solid #3b82f6;
            padding-left: 16px;
            margin: 6px 0 24px 0;
            font-size: 0.9rem;
        }
        .search-area {
            background: #f8fafc;
            border-radius: 20px;
            padding: 20px 24px;
            margin-bottom: 32px;
            display: flex;
            flex-wrap: wrap;
            align-items: flex-end;
            gap: 16px;
            box-shadow: 0 1px 2px rgba(0,0,0,0.03);
            border: 1px solid #e2e8f0;
        }
        .input-group {
            flex: 2;
            min-width: 220px;
        }
        .input-group label {
            display: block;
            font-weight: 500;
            color: #334155;
            margin-bottom: 6px;
            font-size: 0.85rem;
        }
        .input-group input {
            width: 100%;
            padding: 12px 16px;
            border-radius: 16px;
            border: 1.5px solid #cbd5e1;
            font-size: 1rem;
            transition: 0.2s;
            background: white;
        }
        .input-group input:focus {
            outline: none;
            border-color: #3b82f6;
            box-shadow: 0 0 0 3px rgba(59,130,246,0.2);
        }
        button {
            background: #3b82f6;
            border: none;
            color: white;
            padding: 12px 28px;
            border-radius: 40px;
            font-weight: 600;
            font-size: 0.95rem;
            cursor: pointer;
            transition: 0.2s;
            box-shadow: 0 2px 5px rgba(0,0,0,0.05);
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }
        button:hover {
            background: #2563eb;
            transform: translateY(-1px);
            box-shadow: 0 6px 12px rgba(59,130,246,0.2);
        }
        button:active {
            transform: translateY(1px);
        }
        .select-group {
            flex: 1.5;
            min-width: 240px;
        }
        .select-group label {
            font-weight: 500;
            color: #334155;
            margin-bottom: 6px;
            display: block;
            font-size: 0.85rem;
        }
        select {
            width: 100%;
            padding: 12px 16px;
            border-radius: 16px;
            border: 1.5px solid #cbd5e1;
            background: white;
            font-size: 0.95rem;
            cursor: pointer;
            transition: 0.2s;
        }
        select:focus {
            border-color: #3b82f6;
            outline: none;
        }
        .info-badge {
            background: #eef2ff;
            padding: 6px 12px;
            border-radius: 40px;
            font-size: 0.8rem;
            color: #1e40af;
        }
        .card-title {
            font-size: 1.3rem;
            font-weight: 600;
            margin: 24px 0 16px 0;
            color: #0f172a;
            display: flex;
            align-items: center;
            gap: 10px;
            border-bottom: 2px solid #e2e8f0;
            padding-bottom: 8px;
        }
        .table-wrapper {
            overflow-x: auto;
            border-radius: 20px;
            border: 1px solid #eef2ff;
            background: white;
            margin-bottom: 28px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.03);
        }
        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.9rem;
        }
        th {
            background: #f1f5f9;
            padding: 14px 16px;
            text-align: left;
            font-weight: 600;
            color: #1e293b;
            border-bottom: 1px solid #e2e8f0;
        }
        td {
            padding: 12px 16px;
            border-bottom: 1px solid #f0f2f5;
            vertical-align: top;
        }
        tr:last-child td {
            border-bottom: none;
        }
        .task-name {
            font-weight: 600;
            color: #0c4a6e;
            cursor: help;
            border-bottom: 1px dashed #94a3b8;
            position: relative;
            display: inline-block;
        }
        .group-name {
            font-weight: 500;
            color: #2d3c5e;
            cursor: help;
            background: #f1f5f9;
            padding: 4px 8px;
            border-radius: 20px;
            display: inline-block;
            font-size: 0.85rem;
            border-bottom: 1px dashed #64748b;
        }
        /* 浮动层tooltip 自定义 */
        .hover-tooltip {
            position: absolute;
            background: #1e293b;
            color: #f1f5f9;
            padding: 10px 14px;
            border-radius: 12px;
            font-size: 0.8rem;
            max-width: 320px;
            word-wrap: break-word;
            white-space: normal;
            z-index: 1000;
            box-shadow: 0 8px 20px rgba(0,0,0,0.2);
            pointer-events: none;
            transition: opacity 0.1s;
            backdrop-filter: blur(2px);
            line-height: 1.4;
            font-weight: normal;
        }
        .empty-message {
            text-align: center;
            padding: 48px 20px;
            color: #64748b;
            font-style: italic;
        }
        .loading-shade {
            display: inline-block;
            width: 18px;
            height: 18px;
            border: 2px solid #cbd5e1;
            border-top-color: #3b82f6;
            border-radius: 50%;
            animation: spin 0.6s linear infinite;
            margin-right: 8px;
            vertical-align: middle;
        }
        @keyframes spin {
            to { transform: rotate(360deg); }
        }
        footer {
            text-align: center;
            margin-top: 28px;
            font-size: 0.75rem;
            color: #94a3b8;
        }
        hr {
            margin: 12px 0;
            border-color: #eef2ff;
        }
        @media (max-width: 680px) {
            .app-container { padding: 20px; }
            .search-area { flex-direction: column; align-items: stretch; }
            button { justify-content: center; }
        }
    </style>
</head>
<body>
<div id="app">
    <div class="app-container">
        <h1>📋 流程任务编排中心</h1>
        <div class="sub">按流程ID检索 → 查看任务条件映射 → 获取最终优先级合并任务列表及分配组</div>

        <!-- 搜索区域 -->
        <div class="search-area">
            <div class="input-group">
                <label>🔍 流程ID (Process ID)</label>
                <input type="text" v-model="processId" placeholder="例如: PROC_1001, PR-2025" @keyup.enter="searchProcess">
            </div>
            <button @click="searchProcess" :disabled="loading">
                <span v-if="loading" class="loading-shade"></span>
                <span v-else>🔎</span>
                {{ loading ? '查询中...' : '搜索任务' }}
            </button>
            <div class="select-group">
                <label>📌 关联Task条目 (快速定位)</label>
                <select v-model="selectedTaskName" @change="onSelectTaskChange">
                    <option value="">-- 所有任务 --</option>
                    <option v-for="task in taskNameList" :value="task">{{ task }}</option>
                </select>
            </div>
        </div>

        <!-- 表格1: 当前ID匹配到哪些task的condition -->
        <div class="card-title">
            <span>📌 表1 · 任务条件映射</span>
            <span class="info-badge" v-if="conditionTasks.length">共 {{ conditionTasks.length }} 个任务</span>
        </div>
        <div class="table-wrapper">
            <table v-if="conditionTasks.length > 0">
                <thead>
                <tr><th>Task 名称</th><th>关联条件 (Condition) 明细</th></tr>
                </thead>
                <tbody>
                    <tr v-for="item in filteredConditionTasks" :key="item.taskId">
                        <td style="width: 30%;">
                            <span class="task-name" 
                                  @mouseenter="showTooltip($event, formatConditionTooltip(item.conditions))"
                                  @mouseleave="hideTooltip">
                                {{ item.taskName }}
                            </span>
                        </td>
                        <td>
                            <div v-for="(cond, idx) in item.conditions" :key="idx" style="margin-bottom: 6px;">
                                🔹 {{ cond.conditionKey }} : {{ cond.conditionValue }}
                                <span v-if="cond.operator"> ({{ cond.operator }})</span>
                            </div>
                            <div v-if="!item.conditions.length" class="empty-message" style="padding:0; text-align:left;">无具体条件</div>
                        </td>
                    </tr>
                </tbody>
            </table>
            <div v-else class="empty-message">
                ⚡ 暂无任务条件数据，请先输入流程ID并点击搜索。
            </div>
        </div>

        <!-- 表格二: 最终通过优先级和合并以后获取到的最终task列表以及每个task被分配的组名称 -->
        <div class="card-title">
            <span>⚙️ 表2 · 最终任务列表 (优先级合并后) + 分配组</span>
            <span class="info-badge" v-if="finalTasks.length">优先级聚合 · {{ finalTasks.length }} 项</span>
        </div>
        <div class="table-wrapper">
            <table v-if="finalTasks.length > 0">
                <thead>
                    <tr><th style="width:35%">Task 名称</th><th>优先级 (Priority)</th><th style="width:35%">分配的组名称 (Group)</th></tr>
                </thead>
                <tbody>
                    <tr v-for="task in filteredFinalTasks" :key="task.taskId">
                        <td><strong>{{ task.taskName }}</strong></td>
                        <td>
                            <span :class="{'priority-high': task.priority > 5, 'priority-mid': task.priority <=5 && task.priority > 2}">
                                ⭐ {{ task.priority }}
                            </span>
                        </td>
                        <td>
                            <span class="group-name"
                                  @mouseenter="showTooltip($event, formatGroupDetail(task.assignedGroup))"
                                  @mouseleave="hideTooltip">
                                {{ task.assignedGroup.groupName || '未分配' }}
                            </span>
                        </td>
                    </tr>
                </tbody>
            </table>
            <div v-else class="empty-message">
                📭 暂无最终任务数据，请执行搜索并获取后台模拟数据。
            </div>
        </div>
        <footer>💡 提示: 鼠标悬浮在【Task名称】可查看详细条件值匹配；悬浮在【组名称】可查看该组的成员/权限详情。</footer>
    </div>
    <!-- 动态tooltip浮动层容器 -->
    <div id="dynamicTooltip" class="hover-tooltip" style="position:fixed; display:none; z-index:9999;"></div>
</div>

<script>
    // 模拟后台数据服务 (通过Ajax风格, 实际使用$.ajax 模拟)
    // 根据流程ID, 综合多表数据 (表1: 任务条件映射; 表2: 优先级合并后 + 组分配)
    // 为了展示完整逻辑，构建一个异步模拟函数 getTasksByProcessId(processId)
    
    // ---------- 模拟数据库 ----------
    // 预置流程相关数据集:
    // 流程任务条件表 (表1概念: 每个流程下某个task有哪些condition)
    // 最终任务表通过优先级和合并规则得出 (优先级合并: 同一任务可能多条, 取最高优先级; 组根据条件匹配分配)
    
    const mockDatabase = {
        // 流程PROC_1001: 采购流程
        "PROC_1001": {
            // 任务条件映射(原始多表条件)
            taskConditions: [
                { taskId: "T001", taskName: "采购申请审批", conditions: [
                    { conditionKey: "amount", conditionValue: ">5000", operator: ">" },
                    { conditionKey: "dept", conditionValue: "财务部", operator: "=" }
                ]},
                { taskId: "T002", taskName: "合同法务审核", conditions: [
                    { conditionKey: "contract_value", conditionValue: ">=20000", operator: ">=" },
                    { conditionKey: "legal_required", conditionValue: "true", operator: "==" }
                ]},
                { taskId: "T003", taskName: "采购主管复核", conditions: [
                    { conditionKey: "amount", conditionValue: ">1000", operator: ">" }
                ]}
            ],
            // 最终任务列表(经过优先级合并和组分配)
            finalTaskAllocations: [
                { taskId: "T001", taskName: "采购申请审批", priority: 8, assignedGroup: { groupName: "财务审批组", members: "张财务, 李会计", description: "负责金额大于5k的采购单审批" } },
                { taskId: "T002", taskName: "合同法务审核", priority: 9, assignedGroup: { groupName: "法务合规组", members: "王律师, 赵法务", description: "审核合同条款及法律风险" } },
                { taskId: "T003", taskName: "采购主管复核", priority: 5, assignedGroup: { groupName: "采购管理组", members: "孙主管, 周经理", description: "日常采购复核与供应商协调" } }
            ]
        },
        "PR-2025": {
            taskConditions: [
                { taskId: "T101", taskName: "需求分析", conditions: [ { conditionKey: "project_type", conditionValue: "ERP升级", operator: "=" }, { conditionKey: "budget", conditionValue: ">100k", operator: ">" } ] },
                { taskId: "T102", taskName: "技术评审", conditions: [ { conditionKey: "tech_complexity", conditionValue: "高", operator: "=" } ] },
                { taskId: "T103", taskName: "资源分配", conditions: [ { conditionKey: "team_size", conditionValue: ">=5", operator: ">=" } ] }
            ],
            finalTaskAllocations: [
                { taskId: "T101", taskName: "需求分析", priority: 7, assignedGroup: { groupName: "产品规划组", members: "产品经理A, 业务分析师B", description: "负责需求调研与分析文档" } },
                { taskId: "T102", taskName: "技术评审", priority: 9, assignedGroup: { groupName: "架构委员会", members: "架构师X, 技术总监Y", description: "技术方案评审与决策" } },
                { taskId: "T103", taskName: "资源分配", priority: 6, assignedGroup: { groupName: "项目管理办公室(PMO)", members: "PMO专员, 资源经理", description: "协调人力及预算" } }
            ]
        },
        "FLOW_DEMO_88": {
            taskConditions: [
                { taskId: "T201", taskName: "数据预检", conditions: [ { conditionKey: "data_volume", conditionValue: ">10000", operator: ">" }, { conditionKey: "source_type", conditionValue: "Kafka", operator: "=" } ] },
                { taskId: "T202", taskName: "ETL清洗", conditions: [ { conditionKey: "dirty_rate", conditionValue: ">5%", operator: ">" } ] }
            ],
            finalTaskAllocations: [
                { taskId: "T201", taskName: "数据预检", priority: 4, assignedGroup: { groupName: "数据质量组", members: "数据工程师张, 质量专员李", description: "检查数据完整性及异常" } },
                { taskId: "T202", taskName: "ETL清洗", priority: 8, assignedGroup: { groupName: "数据集成组", members: "ETL开发王, 运维刘", description: "数据转换与清洗逻辑实现" } }
            ]
        },
        "DEFAULT_EMPTY": {
            taskConditions: [],
            finalTaskAllocations: []
        }
    };
    
    // 辅助: 获取流程数据 (模拟ajax延迟)
    function fetchProcessData(processId) {
        return new Promise((resolve, reject) => {
            // 模拟网络请求延迟300~600ms
            setTimeout(() => {
                // 大小写不敏感但为了演示，保留原始key匹配
                let data = mockDatabase[processId];
                if (!data) {
                    // 如果不存在则返回空数据，但给一个友好提示（不reject，让页面显示空表格）
                    data = mockDatabase["DEFAULT_EMPTY"];
                }
                // 深拷贝，避免引用修改
                const cloned = {
                    taskConditions: JSON.parse(JSON.stringify(data.taskConditions)),
                    finalTaskAllocations: JSON.parse(JSON.stringify(data.finalTaskAllocations))
                };
                resolve(cloned);
            }, 400);
        });
    }
    
    // Vue实例
    new Vue({
        el: '#app',
        data: {
            processId: '',           // 用户输入的流程ID
            loading: false,
            // 表1原始条件任务列表
            conditionTasks: [],      // 结构: [{taskId, taskName, conditions:[]}]
            // 表2最终任务列表
            finalTasks: [],          // 结构: [{taskId, taskName, priority, assignedGroup:{groupName, members, description}}]
            selectedTaskName: '',    // 下拉过滤任务名
            tooltipTimer: null
        },
        computed: {
            // 获取所有任务名称集合 (用于下拉选择)
            taskNameList: function() {
                let names = [];
                this.conditionTasks.forEach(t => {
                    if(t.taskName) names.push(t.taskName);
                });
                // 同时从最终任务里补全可能名称 (但表1基本覆盖所有出现任务)
                this.finalTasks.forEach(t => {
                    if(t.taskName && !names.includes(t.taskName)) names.push(t.taskName);
                });
                return [...new Set(names)]; // 去重
            },
            // 过滤表1（根据select选中的任务名）
            filteredConditionTasks: function() {
                if(!this.selectedTaskName) return this.conditionTasks;
                return this.conditionTasks.filter(task => task.taskName === this.selectedTaskName);
            },
            // 过滤最终任务列表（根据选中的任务名）
            filteredFinalTasks: function() {
                if(!this.selectedTaskName) return this.finalTasks;
                return this.finalTasks.filter(task => task.taskName === this.selectedTaskName);
            }
        },
        methods: {
            searchProcess: function() {
                let pid = this.processId.trim();
                if(pid === "") {
                    alert("请输入流程ID (例如 PROC_1001, PR-2025, FLOW_DEMO_88)");
                    return;
                }
                this.loading = true;
                // 调用模拟Ajax获取多表数据
                fetchProcessData(pid).then(res => {
                    // 更新表1 和 表2
                    this.conditionTasks = res.taskConditions || [];
                    this.finalTasks = res.finalTaskAllocations || [];
                    // 重置选中的task过滤项
                    this.selectedTaskName = '';
                    this.loading = false;
                    // 如果数据为空给出提示(可隐式)
                    if(this.conditionTasks.length === 0 && this.finalTasks.length === 0) {
                        alert(`未找到流程ID "${pid}" 的相关任务数据，可尝试: PROC_1001, PR-2025, FLOW_DEMO_88`);
                    }
                }).catch(err => {
                    console.error(err);
                    this.loading = false;
                    alert("请求异常，请重试");
                });
            },
            // 当下拉选择发生变化时可高亮对应区域（前端自动过滤），可选额外滚动效果
            onSelectTaskChange: function() {
                if(this.selectedTaskName) {
                    // 简单定位: 不用滚动，让表格过滤即可
                }
            },
            // 格式化task条件展示tooltip内容 (显示多个条件数值匹配)
            formatConditionTooltip: function(conditions) {
                if(!conditions || conditions.length === 0) return "无特定条件约束";
                let lines = conditions.map(cond => {
                    return `${cond.conditionKey} ${cond.operator || '='} ${cond.conditionValue}`;
                });
                return "📐 条件匹配规则:\n" + lines.join("\n");
            },
            // 格式化组的详情tooltip
            formatGroupDetail: function(group) {
                if(!group || !group.groupName) return "未分配组";
                let details = `🏢 组名称: ${group.groupName}\n👥 成员: ${group.members || '未指定'}\n📝 职责: ${group.description || '无描述'}`;
                return details;
            },
            // 展示浮动层 (全局tooltip)
            showTooltip: function(event, content) {
                const tooltipEl = document.getElementById('dynamicTooltip');
                if(!tooltipEl) return;
                // 清空之前的延迟隐藏
                if(this.tooltipTimer) clearTimeout(this.tooltipTimer);
                tooltipEl.innerHTML = content.replace(/\n/g, '<br>');
                tooltipEl.style.display = 'block';
                // 计算位置 (基于鼠标)
                let x = event.clientX + 15;
                let y = event.clientY + 15;
                // 边界粗略防止超出右侧
                const rect = tooltipEl.getBoundingClientRect();
                // 因为display刚刚显示，没有实际宽高，可异步获取尺寸
                setTimeout(() => {
                    const finalRect = tooltipEl.getBoundingClientRect();
                    let left = x;
                    let top = y;
                    if(left + finalRect.width > window.innerWidth - 10) {
                        left = event.clientX - finalRect.width - 10;
                    }
                    if(top + finalRect.height > window.innerHeight - 10) {
                        top = event.clientY - finalRect.height - 10;
                    }
                    tooltipEl.style.left = left + 'px';
                    tooltipEl.style.top = top + 'px';
                }, 10);
            },
            hideTooltip: function() {
                const tooltipEl = document.getElementById('dynamicTooltip');
                if(tooltipEl) {
                    // 加一点延迟以避免快速移动闪烁
                    this.tooltipTimer = setTimeout(() => {
                        if(tooltipEl) tooltipEl.style.display = 'none';
                    }, 100);
                }
            }
        },
        mounted() {
            // 初始时可以默认请求一个样例使页面不空，或者展示占位，但保留用户主动搜索
            // 为了体验，首次自动展示一个示例? 也可设置默认流程ID，但最好让用户自己操作，仅预填充占位符
            this.processId = "PROC_1001";
            // 自动加载示例 (展示说明)
            this.searchProcess();
        },
        // 清理tooltip延迟
        beforeDestroy() {
            if(this.tooltipTimer) clearTimeout(this.tooltipTimer);
        }
    });
    
    // 附加全局鼠标移动时可能微调tooltip位置已在show中处理。
    // 确保点击页面其他地方可隐藏没必要处理
</script>
</body>
</html>
