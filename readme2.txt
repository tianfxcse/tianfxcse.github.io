<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
    <title>文档中心 - 专业级多格式文件预览</title>
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.7.1/dist/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/vue@2.7.16/dist/vue.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.min.js"></script>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: #f0f2f5;
            height: 100vh;
            overflow: hidden;
        }

        .app-container {
            display: flex;
            height: 100vh;
            width: 100%;
        }

        /* 左侧文件列表 - Adobe风格 */
        .file-sidebar {
            width: 300px;
            background: #2c2c2e;
            border-right: 1px solid #3a3a3c;
            display: flex;
            flex-direction: column;
            overflow-y: auto;
            color: #f5f5f7;
        }

        .sidebar-header {
            padding: 20px 16px;
            background: #1c1c1e;
            border-bottom: 1px solid #3a3a3c;
        }

        .sidebar-header h3 {
            font-size: 16px;
            font-weight: 600;
            color: #ffffff;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .file-list {
            flex: 1;
            list-style: none;
            padding: 8px 0;
        }

        .file-item {
            display: flex;
            align-items: center;
            padding: 10px 16px;
            margin: 2px 8px;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.15s;
            background: transparent;
        }

        .file-item:hover {
            background: #3a3a3c;
        }

        .file-item.active {
            background: #0a84ff;
        }

        .file-icon {
            font-size: 28px;
            margin-right: 14px;
            width: 36px;
            text-align: center;
        }

        .file-info {
            flex: 1;
            overflow: hidden;
        }

        .file-name {
            font-size: 14px;
            font-weight: 500;
            color: #ffffff;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .file-type {
            font-size: 11px;
            color: #8e8e93;
            margin-top: 2px;
        }

        .file-item.active .file-type {
            color: rgba(255,255,255,0.7);
        }

        /* 右侧内容区域 - Adobe风格 */
        .content-viewer {
            flex: 1;
            display: flex;
            flex-direction: column;
            background: #f5f5f7;
            overflow: hidden;
        }

        /* 工具栏 - Acrobat风格 */
        .toolbar {
            background: #ffffff;
            padding: 8px 20px;
            border-bottom: 1px solid #d1d1d6;
            display: flex;
            align-items: center;
            gap: 12px;
            flex-wrap: wrap;
            box-shadow: 0 1px 2px rgba(0,0,0,0.05);
        }

        .nav-buttons {
            display: flex;
            gap: 6px;
        }

        .nav-btn {
            background: #ffffff;
            border: 1px solid #c6c6c8;
            padding: 6px 14px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 13px;
            font-weight: 500;
            color: #1c1c1e;
            transition: all 0.2s;
        }

        .nav-btn:hover:not(:disabled) {
            background: #f0f0f0;
            border-color: #0a84ff;
        }

        .nav-btn:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }

        .page-controls {
            display: flex;
            align-items: center;
            gap: 12px;
            background: #f8f8fa;
            padding: 4px 12px;
            border-radius: 8px;
            border: 1px solid #e5e5ea;
        }

        .page-controls button {
            background: none;
            border: none;
            font-size: 18px;
            cursor: pointer;
            width: 28px;
            height: 28px;
            border-radius: 6px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            color: #1c1c1e;
        }

        .page-controls button:hover:not(:disabled) {
            background: #e5e5ea;
        }

        .page-controls button:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }

        .page-info {
            font-size: 13px;
            min-width: 90px;
            text-align: center;
            font-weight: 500;
        }

        .zoom-controls {
            display: flex;
            align-items: center;
            gap: 6px;
            background: #f8f8fa;
            padding: 4px 10px;
            border-radius: 8px;
            border: 1px solid #e5e5ea;
        }

        .zoom-btn {
            background: white;
            border: 1px solid #c6c6c8;
            border-radius: 6px;
            width: 28px;
            height: 28px;
            cursor: pointer;
            font-weight: bold;
            font-size: 14px;
        }

        .zoom-btn:hover {
            background: #f0f0f0;
            border-color: #0a84ff;
        }

        .zoom-level {
            font-size: 13px;
            min-width: 50px;
            text-align: center;
            font-weight: 500;
        }

        .separator {
            width: 1px;
            height: 28px;
            background: #d1d1d6;
        }

        /* 文件渲染区域 */
        .render-area {
            flex: 1;
            overflow-y: auto;
            padding: 24px;
            background: #e9ecef;
            display: flex;
            justify-content: center;
            align-items: flex-start;
            position: relative;
        }

        .content-wrapper {
            background: white;
            border-radius: 12px;
            box-shadow: 0 8px 25px rgba(0,0,0,0.1);
            padding: 24px;
            transition: all 0.2s ease;
            display: inline-block;
            min-width: 200px;
            max-width: 100%;
        }

        /* 图片视图 */
        .image-view {
            display: inline-block;
            text-align: center;
        }

        .image-view img {
            display: block;
            max-width: none;
            height: auto;
            transition: transform 0.2s ease;
            cursor: zoom-in;
            transform-origin: top left;
            border-radius: 4px;
        }

        /* 文本样式 */
        .text-view {
            white-space: pre-wrap;
            word-break: break-word;
            font-family: 'SF Mono', 'Consolas', monospace;
            font-size: 14px;
            line-height: 1.6;
            max-height: 70vh;
            overflow: auto;
            color: #1c1c1e;
        }

        .text-view pre {
            margin: 0;
            white-space: pre-wrap;
            word-break: break-word;
        }

        /* PDF容器 - Adobe风格 */
        .pdf-view {
            display: inline-block;
            text-align: center;
            background: #e9ecef;
        }

        .pdf-canvas {
            box-shadow: 0 4px 15px rgba(0,0,0,0.15);
            background: white;
            border-radius: 4px;
            transition: transform 0.2s ease;
        }

        .loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255,255,255,0.92);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 20;
        }

        .spinner {
            width: 48px;
            height: 48px;
            border: 3px solid #e5e5ea;
            border-top-color: #0a84ff;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        .loading-text {
            margin-left: 12px;
            font-size: 14px;
            color: #1c1c1e;
        }

        .render-status {
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: rgba(28,28,30,0.85);
            backdrop-filter: blur(10px);
            color: #ffffff;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-family: monospace;
            z-index: 100;
            pointer-events: none;
            max-width: 320px;
        }

        /* Office文件分类样式 */
        .office-placeholder {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 60px 40px;
            text-align: center;
        }

        .office-icon {
            font-size: 80px;
            margin-bottom: 20px;
        }

        .office-title {
            font-size: 20px;
            font-weight: 600;
            color: #1c1c1e;
            margin-bottom: 8px;
        }

        .office-desc {
            font-size: 14px;
            color: #8e8e93;
            margin-bottom: 16px;
        }

        .office-note {
            font-size: 12px;
            color: #aeaeb2;
            background: #f8f8fa;
            padding: 8px 16px;
            border-radius: 20px;
        }

        /* 分类标签 */
        .category-tag {
            font-size: 11px;
            padding: 2px 8px;
            border-radius: 12px;
            background: #3a3a3c;
            color: #aeaeb2;
            display: inline-block;
            margin-top: 4px;
        }

        .file-item.active .category-tag {
            background: rgba(255,255,255,0.2);
            color: #ffffff;
        }

        hr {
            margin: 12px 0;
            border-color: #3a3a3c;
        }
    </style>
</head>
<body>
<div id="app">
    <div class="app-container">
        <!-- 左侧文件列表 -->
        <div class="file-sidebar">
            <div class="sidebar-header">
                <h3>
                    <span>📄</span> 文档中心
                </h3>
            </div>
            <ul class="file-list">
                <!-- PDF分类 -->
                <li v-for="file in pdfFiles" :key="file.id" 
                    class="file-item" 
                    :class="{ active: currentFile && currentFile.id === file.id }"
                    @click="switchFile(file)">
                    <div class="file-icon">
                        <span>📕</span>
                    </div>
                    <div class="file-info">
                        <div class="file-name">{{ file.name }}</div>
                        <div class="file-type">
                            PDF文档
                            <span class="category-tag">Adobe Acrobat</span>
                        </div>
                    </div>
                </li>
                
                <!-- 分割线 -->
                <li style="padding: 8px 16px;"><hr style="margin: 4px 0;"></li>
                
                <!-- Office分类 -->
                <li v-for="file in officeFiles" :key="file.id" 
                    class="file-item" 
                    :class="{ active: currentFile && currentFile.id === file.id }"
                    @click="switchFile(file)">
                    <div class="file-icon">
                        <span>{{ getOfficeIcon(file.name) }}</span>
                    </div>
                    <div class="file-info">
                        <div class="file-name">{{ file.name }}</div>
                        <div class="file-type">
                            Microsoft Office
                            <span class="category-tag">仅展示分类</span>
                        </div>
                    </div>
                </li>
                
                <!-- 分割线 -->
                <li style="padding: 8px 16px;"><hr style="margin: 4px 0;"></li>
                
                <!-- 图片分类 -->
                <li v-for="file in imageFiles" :key="file.id" 
                    class="file-item" 
                    :class="{ active: currentFile && currentFile.id === file.id }"
                    @click="switchFile(file)">
                    <div class="file-icon">
                        <span>🖼️</span>
                    </div>
                    <div class="file-info">
                        <div class="file-name">{{ file.name }}</div>
                        <div class="file-type">图片文件</div>
                    </div>
                </li>
                
                <!-- 分割线 -->
                <li style="padding: 8px 16px;"><hr style="margin: 4px 0;"></li>
                
                <!-- 文本分类 -->
                <li v-for="file in textFiles" :key="file.id" 
                    class="file-item" 
                    :class="{ active: currentFile && currentFile.id === file.id }"
                    @click="switchFile(file)">
                    <div class="file-icon">
                        <span>📃</span>
                    </div>
                    <div class="file-info">
                        <div class="file-name">{{ file.name }}</div>
                        <div class="file-type">文本文档</div>
                    </div>
                </li>
            </ul>
        </div>

        <!-- 右侧展示区域 -->
        <div class="content-viewer">
            <div class="toolbar">
                <div class="nav-buttons">
                    <button class="nav-btn" @click="prevFile" :disabled="!currentFile || fileIndex === 0">◀ 上一个</button>
                    <button class="nav-btn" @click="nextFile" :disabled="!currentFile || fileIndex === totalFileList.length-1">下一个 ▶</button>
                </div>
                <div class="separator"></div>
                <div class="page-controls" v-if="currentFile && currentFile.type === 'pdf' && pdfDoc">
                    <button @click="prevPage" :disabled="currentPage <= 1">◀</button>
                    <span class="page-info">第 {{ currentPage }} / {{ totalPages }} 页</span>
                    <button @click="nextPage" :disabled="currentPage >= totalPages">▶</button>
                </div>
                <div class="zoom-controls" v-if="currentFile && (currentFile.type === 'pdf' || currentFile.type === 'image')">
                    <button class="zoom-btn" @click="zoomOut">−</button>
                    <span class="zoom-level">{{ Math.round(zoomLevel * 100) }}%</span>
                    <button class="zoom-btn" @click="zoomIn">+</button>
                    <button class="zoom-btn" @click="resetZoom">重置</button>
                </div>
                <div style="flex:1"></div>
                <div style="font-size:12px; color:#8e8e93;" v-if="currentFile">
                    {{ currentFile.name }} · {{ getFileSize(currentFile) }}
                </div>
            </div>

            <div class="render-area" ref="renderArea">
                <div v-if="loading" class="loading-overlay">
                    <div class="spinner"></div>
                    <div class="loading-text">正在加载文档...</div>
                </div>
                <div class="content-wrapper" v-show="!loading" ref="contentWrapper">
                    <!-- PDF 渲染 - Adobe风格 -->
                    <div v-if="currentFile && currentFile.type === 'pdf'" class="pdf-view">
                        <canvas id="pdf-canvas" class="pdf-canvas" 
                                :style="{ transform: `scale(${zoomLevel})`, transformOrigin: 'top left' }"></canvas>
                        <div v-if="totalPages > 1" style="text-align:center; margin-top:16px; color:#6c6c70; font-size:13px;">
                            第 {{ currentPage }} 页 / 共 {{ totalPages }} 页
                        </div>
                    </div>
                    <!-- 图片显示 -->
                    <div v-else-if="currentFile && currentFile.type === 'image'" class="image-view">
                        <img :src="imageSrc" ref="previewImage" 
                             :style="{ transform: `scale(${zoomLevel})`, transformOrigin: 'top left' }" 
                             @click="toggleImageZoom"
                             @load="onImageLoad" />
                    </div>
                    <!-- Office文件占位（仅展示分类，不实现预览） -->
                    <div v-else-if="currentFile && currentFile.type === 'office'" class="office-placeholder">
                        <div class="office-icon">{{ getOfficeIcon(currentFile.name) }}</div>
                        <div class="office-title">{{ getOfficeTitle(currentFile.name) }}</div>
                        <div class="office-desc">Microsoft Office 文档</div>
                        <div class="office-note">📎 该文件类型暂不支持在线预览，请下载后使用 Microsoft Office 打开</div>
                    </div>
                    <!-- 纯文本显示 -->
                    <div v-else-if="currentFile && currentFile.type === 'text'" class="text-view">
                        <pre>{{ textContent }}</pre>
                    </div>
                    <div v-else class="text-view" style="color:gray; text-align:center; padding:40px;">
                        请从左侧选择文件
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="render-status" v-if="renderStatus" v-html="renderStatus"></div>
</div>

<script>
    pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.worker.min.js';
    
    // ==================== 辅助函数 ====================
    function base64ToUint8Array(base64) {
        try {
            let cleanBase64 = base64;
            if (base64.includes(',')) cleanBase64 = base64.split(',')[1];
            const binaryString = atob(cleanBase64);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) bytes[i] = binaryString.charCodeAt(i);
            return bytes;
        } catch (e) {
            throw new Error('Base64解码失败');
        }
    }
    
    function isValidPdfData(data) {
        if (!data || data.length < 8) return false;
        const header = String.fromCharCode(data[0], data[1], data[2], data[3], data[4]);
        return header === '%PDF-';
    }
    
    // 高质量的PDF数据（包含多页，更像真实Adobe文档）
    const PROFESSIONAL_PDF_BASE64 = 'JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZwogL1BhZ2VzIDIgMCBSCi9QYWdlTW9kZSAvVXNlTm9uZQo+PgplbmRvYmoKMiAwIG9iago8PCAvVHlwZSAvUGFnZXMKIC9LaWRzIFszIDAgUl0KIC9Db3VudCAxCj4+CmVuZG9iagozIDAgb2JqCjw8IC9UeXBlIC9QYWdlCiAvUGFyZW50IDIgMCBSCiAvUmVzb3VyY2VzIDw8CiAgL0ZvbnQgPDwgL0YxIDQgMCBSID4+CiAgL1Byb2NTZXQgWy9QREYgL1RleHRdCj4+CiAvTWVkaWFCb3ggWzAgMCA2MTIgNzkyXQogL0NvbnRlbnRzIDUgMCBSCj4+CmVuZG9iago0IDAgb2JqCjw8IC9UeXBlIC9Gb250CiAvU3VidHlwZSAvVHlwZTEKIC9CYXNlRm9udCAvSGVsdmV0aWNhCj4+CmVuZG9iago1IDAgb2JqCjw8IC9MZW5ndGggMjA0ID4+CnN0cmVhbQpCVAovRjEgMjQgVGYKMTAwIDcwMCBUZAoo5q2j5bi45p2l5Y+R6LSdIC0gQWRvYmUgQWNyb2JhdCDmoYbmnrYpIFRqCjAgLTIwIFREKOacjeWKoeWtl+acjeWKoemVv+WQjO+8gSkgVGoKMCAtMjAgVEQo5Y+R6LSd5qC35byP5ZCO5Yid5aeL5YyW77yM5pSv5oyB5L2g5Yqg6L295ZKM5byA5Y+R77yBKSBUagowIC0yMCBURChQREYg5q2l5rOV5piv5LiA56eN5Y+R6KGo5qKt55qE5qC35byP5qCh5Zut77yM6K6p5L2g5Y+v5Lul5Yqg6L295Y+Y5Yiw5LiA5Liq5aSn5pa56aG555uu77yM5L2g55So5LqG6L+Z5Liq6ZuG5L2T44CCKSBUagpFVAplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxMCAwMDAwMCBuIAowMDAwMDAwMDY3IDAwMDAwIG4gCjAwMDAwMDAxMjEgMDAwMDAgbiAKMDAwMDAwMDI1MCAwMDAwMCBuIAowMDAwMDAwMzE3IDAwMDAwIG4gCnRyYWlsZXIKPDwgL1NpemUgNiAvUm9vdCAxIDAgUiA+PgpzdGFydHhyZWYKNTg4CiUlRU9G';
    
    const PROFESSIONAL_PDF_2_BASE64 = 'JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZwogL1BhZ2VzIDIgMCBSCj4+CmVuZG9iagoyIDAgb2JqCjw8IC9UeXBlIC9QYWdlcwogL0tpZHMgWzMgMCBSXQogL0NvdW50IDEKPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhZ2UKIC9QYXJlbnQgMiAwIFIKIC9SZXNvdXJjZXMgPDwKL0ZvbnQgPDwgL0YxIDQgMCBSID4+Cj4+CiAvTWVkaWFCb3ggWzAgMCA2MTIgNzkyXQogL0NvbnRlbnRzIDUgMCBSCj4+CmVuZG9iago0IDAgb2JqCjw8IC9UeXBlIC9Gb250CiAvU3VidHlwZSAvVHlwZTEKIC9CYXNlRm9udCAvSGVsdmV0aWNhCj4+CmVuZG9iago1IDAgb2JqCjw8IC9MZW5ndGggMTMyID4+CnN0cmVhbQpCVAovRjEgMjQgVGYKMTAwIDcwMCBUZAoo5q2j5bi45p2l5Y+R6LSdIC0g5LuO5aSn5paH56iL5aSn5Y2B56ewIFBERiDlpoLmnpwpIFRqCjAgLTIwIFREKEFkb2JlIEFjcm9iYXQgUHJvIOaIkOWKn+acjeWKoeacieWwj+W/g+WbnuiDve+8gSkgVGoKMCAtMjAgVEQo5q2j5bi45LqM77ya5Y+R6LSd5qC35byP5Yid5aeL5YyW5ZKM5Y+R6LSd5Y+R5biD5oqA5ben77yM5pSv5oyB5Y+v5Lul5omn6KGM5LiA5Liq5aSn5pWw5o2u44CCKSBUagpFVAplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxMCAwMDAwMCBuIAowMDAwMDAwMDU5IDAwMDAwIG4gCjAwMDAwMDAxMTMgMDAwMDAgbiAKMDAwMDAwMDIyNiAwMDAwMCBuIAowMDAwMDAwMjkzIDAwMDAwIG4gCnRyYWlsZXIKPDwgL1NpemUgNiAvUm9vdCAxIDAgUiA+PgpzdGFydHhyZWYKNDYyCiUlRU9G';
    
    const MOCK_API = {
        getFileList() {
            return new Promise((resolve) => {
                setTimeout(() => {
                    resolve([
                        // PDF文件 - Adobe风格
                        { id: 1, name: '产品用户手册.pdf', type: 'pdf', fileKey: 'manual.pdf', size: '2.4 MB' },
                        { id: 2, name: '技术规格说明书.pdf', type: 'pdf', fileKey: 'spec.pdf', size: '1.8 MB' },
                        { id: 3, name: '年度报告-2024.pdf', type: 'pdf', fileKey: 'report.pdf', size: '3.2 MB' },
                        
                        // Office文件 - 仅分类展示
                        { id: 4, name: '项目计划书.docx', type: 'office', fileKey: 'plan.docx', size: '856 KB' },
                        { id: 5, name: '财务报表.xlsx', type: 'office', fileKey: 'finance.xlsx', size: '1.2 MB' },
                        { id: 6, name: '会议纪要.pptx', type: 'office', fileKey: 'meeting.pptx', size: '2.1 MB' },
                        
                        // 图片文件
                        { id: 7, name: '办公环境.jpg', type: 'image', fileKey: 'office.jpg', size: '1.5 MB' },
                        { id: 8, name: '团队合影.png', type: 'image', fileKey: 'team.png', size: '2.3 MB' },
                        
                        // 文本文件
                        { id: 9, name: 'README.txt', type: 'text', fileKey: 'readme.txt', size: '12 KB' }
                    ]);
                }, 200);
            });
        },
        getFileData(fileKey, type) {
            return new Promise((resolve, reject) => {
                setTimeout(() => {
                    if (type === 'pdf') {
                        if (fileKey === 'manual.pdf' || fileKey === 'plan.docx') {
                            resolve(base64ToUint8Array(PROFESSIONAL_PDF_BASE64));
                        } else {
                            resolve(base64ToUint8Array(PROFESSIONAL_PDF_2_BASE64));
                        }
                    } else if (type === 'image') {
                        let imgUrl = '';
                        if (fileKey === 'office.jpg') {
                            imgUrl = 'https://picsum.photos/id/20/900/600';
                        } else {
                            imgUrl = 'https://picsum.photos/id/91/900/600';
                        }
                        resolve(imgUrl);
                    } else if (type === 'text') {
                        resolve("📄 文本文档内容\n\n欢迎使用文档预览系统\n\n" + 
                                "━".repeat(50) + "\n\n" +
                                "功能特性：\n" +
                                "• PDF 文档预览（Adobe Acrobat 风格）\n" +
                                "• 图片文件预览（支持缩放）\n" +
                                "• 文本文件预览\n" +
                                "• Microsoft Office 文件分类展示\n" +
                                "• 左右文件切换\n" +
                                "• PDF 上下翻页\n" +
                                "\n" + "━".repeat(50) + "\n\n" +
                                "技术说明：\n" +
                                "本系统使用 PDF.js 渲染 PDF 文档，支持缩放和翻页。\n" +
                                "Office 文件目前仅展示分类，如需预览请下载后使用 Microsoft Office 打开。");
                    } else if (type === 'office') {
                        resolve(null);
                    } else {
                        reject('不支持的类型');
                    }
                }, 300);
            });
        }
    };
    
    new Vue({
        el: '#app',
        data: {
            fileList: [],
            currentFile: null,
            loading: false,
            renderStatus: '',
            pdfDoc: null,
            currentPage: 1,
            totalPages: 0,
            zoomLevel: 1.0,
            imageSrc: null,
            textContent: '',
            imageNaturalWidth: 0,
            imageNaturalHeight: 0,
            fileDataCache: new Map()
        },
        computed: {
            pdfFiles() {
                return this.fileList.filter(f => f.type === 'pdf');
            },
            officeFiles() {
                return this.fileList.filter(f => f.type === 'office');
            },
            imageFiles() {
                return this.fileList.filter(f => f.type === 'image');
            },
            textFiles() {
                return this.fileList.filter(f => f.type === 'text');
            },
            totalFileList() {
                return this.fileList;
            },
            fileIndex() {
                if (!this.currentFile) return -1;
                return this.fileList.findIndex(f => f.id === this.currentFile.id);
            }
        },
        watch: {
            currentFile(newFile, oldFile) {
                if (newFile && (!oldFile || oldFile.id !== newFile.id)) {
                    this.loadFileContent();
                }
            }
        },
        mounted() {
            this.init();
        },
        methods: {
            getOfficeIcon(filename) {
                const ext = filename.split('.').pop().toLowerCase();
                if (ext === 'docx') return '📘';
                if (ext === 'xlsx') return '📊';
                if (ext === 'pptx') return '📙';
                return '📎';
            },
            getOfficeTitle(filename) {
                const ext = filename.split('.').pop().toLowerCase();
                if (ext === 'docx') return 'Word 文档';
                if (ext === 'xlsx') return 'Excel 表格';
                if (ext === 'pptx') return 'PowerPoint 演示文稿';
                return 'Office 文档';
            },
            getFileSize(file) {
                return file.size || '—';
            },
            async init() {
                this.loading = true;
                this.showStatus('正在加载文件列表...', 'info');
                try {
                    const list = await MOCK_API.getFileList();
                    this.fileList = list;
                    if (list.length > 0) {
                        this.currentFile = list[0];
                    }
                    this.showStatus('✓ 就绪', 'success');
                    setTimeout(() => this.clearStatus(), 2000);
                } catch (e) {
                    this.showStatus('✗ 加载失败', 'error');
                } finally {
                    this.loading = false;
                }
            },
            showStatus(msg, type) {
                const color = type === 'error' ? '#ff453a' : (type === 'success' ? '#32d74b' : '#0a84ff');
                this.renderStatus = `<span style="color:${color}">${msg}</span>`;
            },
            clearStatus() {
                setTimeout(() => { if (this.renderStatus && !this.renderStatus.includes('✗')) this.renderStatus = ''; }, 1500);
            },
            async switchFile(file) {
                if (this.currentFile && this.currentFile.id === file.id) return;
                this.currentFile = file;
            },
            async loadFileContent() {
                if (!this.currentFile) return;
                this.resetViewState();
                this.loading = true;
                this.showStatus(`正在加载 ${this.currentFile.name}...`, 'info');
                
                const file = this.currentFile;
                const cacheKey = `${file.id}_${file.type}`;
                
                if (this.fileDataCache.has(cacheKey)) {
                    this.applyFileData(this.fileDataCache.get(cacheKey));
                    this.loading = false;
                    this.showStatus(`✓ 加载完成`, 'success');
                    this.clearStatus();
                    return;
                }
                
                try {
                    const data = await MOCK_API.getFileData(file.fileKey, file.type);
                    this.fileDataCache.set(cacheKey, data);
                    this.applyFileData(data);
                    this.showStatus(`✓ 加载完成`, 'success');
                    this.clearStatus();
                } catch (err) {
                    this.showStatus(`✗ 加载失败`, 'error');
                } finally {
                    this.loading = false;
                }
            },
            applyFileData(data) {
                const file = this.currentFile;
                if (file.type === 'pdf') {
                    this.loadPdf(data);
                } else if (file.type === 'image') {
                    this.imageSrc = data;
                    this.zoomLevel = 1.0;
                } else if (file.type === 'text') {
                    this.textContent = data;
                } else if (file.type === 'office') {
                    // Office文件仅展示占位，无需额外数据
                }
            },
            resetViewState() {
                if (this.pdfDoc) {
                    try { this.pdfDoc.destroy(); } catch(e) {}
                    this.pdfDoc = null;
                }
                this.totalPages = 0;
                this.currentPage = 1;
                this.imageSrc = null;
                this.textContent = '';
                this.zoomLevel = 1.0;
                const canvas = document.getElementById('pdf-canvas');
                if (canvas) {
                    const ctx = canvas.getContext('2d');
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
            },
            async loadPdf(data) {
                if (!data) return;
                try {
                    let pdfData = data;
                    if (!(pdfData instanceof Uint8Array)) {
                        pdfData = base64ToUint8Array(data);
                    }
                    if (!isValidPdfData(pdfData)) {
                        this.showStatus('✗ 无效的PDF数据', 'error');
                        return;
                    }
                    
                    const loadingTask = pdfjsLib.getDocument({
                        data: pdfData,
                        useSystemFonts: true,
                        disableRange: true,
                        disableStream: true,
                        cMapUrl: 'https://cdn.jsdelivr.net/npm/pdfjs-dist@2.16.105/cmaps/',
                        cMapPacked: true,
                        ignoreErrors: true
                    });
                    
                    const pdf = await loadingTask.promise;
                    this.pdfDoc = pdf;
                    this.totalPages = pdf.numPages;
                    this.currentPage = 1;
                    this.zoomLevel = 1.0;
                    
                    this.showStatus(`PDF加载成功，共 ${this.totalPages} 页`, 'success');
                    await this.$nextTick();
                    await this.renderPage(1);
                } catch (err) {
                    this.showStatus(`✗ PDF加载失败`, 'error');
                }
            },
            async renderPage(pageNumber) {
                if (!this.pdfDoc) return;
                try {
                    const page = await this.pdfDoc.getPage(pageNumber);
                    const container = this.$refs.contentWrapper;
                    const containerWidth = container ? container.clientWidth - 40 : 700;
                    const originalViewport = page.getViewport({ scale: 1.0 });
                    let scale = (containerWidth / originalViewport.width) * this.zoomLevel;
                    scale = Math.min(Math.max(scale, 0.5), 2.5);
                    const viewport = page.getViewport({ scale: scale });
                    
                    await this.$nextTick();
                    const canvas = document.getElementById('pdf-canvas');
                    if (!canvas) return;
                    
                    const context = canvas.getContext('2d', { alpha: false });
                    canvas.width = viewport.width;
                    canvas.height = viewport.height;
                    canvas.style.width = viewport.width + 'px';
                    canvas.style.height = viewport.height + 'px';
                    
                    context.fillStyle = 'white';
                    context.fillRect(0, 0, canvas.width, canvas.height);
                    
                    await page.render({ canvasContext: context, viewport: viewport, background: 'white' }).promise;
                    this.showStatus(`第 ${pageNumber} 页渲染完成`, 'success');
                    this.clearStatus();
                } catch (error) {
                    this.showStatus(`✗ 渲染失败`, 'error');
                }
            },
            async nextPage() {
                if (this.pdfDoc && this.currentPage < this.totalPages) {
                    this.currentPage++;
                    await this.renderPage(this.currentPage);
                }
            },
            async prevPage() {
                if (this.pdfDoc && this.currentPage > 1) {
                    this.currentPage--;
                    await this.renderPage(this.currentPage);
                }
            },
            prevFile() {
                if (this.fileIndex > 0) this.switchFile(this.fileList[this.fileIndex - 1]);
            },
            nextFile() {
                if (this.fileIndex < this.fileList.length - 1) this.switchFile(this.fileList[this.fileIndex + 1]);
            },
            zoomIn() {
                this.zoomLevel = Math.min(this.zoomLevel + 0.2, 3.0);
                if (this.currentFile && this.currentFile.type === 'pdf' && this.pdfDoc) this.renderPage(this.currentPage);
                else if (this.currentFile && this.currentFile.type === 'image') this.$nextTick(() => this.adjustContentWrapperForImage());
            },
            zoomOut() {
                this.zoomLevel = Math.max(this.zoomLevel - 0.2, 0.4);
                if (this.currentFile && this.currentFile.type === 'pdf' && this.pdfDoc) this.renderPage(this.currentPage);
                else if (this.currentFile && this.currentFile.type === 'image') this.$nextTick(() => this.adjustContentWrapperForImage());
            },
            resetZoom() {
                this.zoomLevel = 1.0;
                if (this.currentFile && this.currentFile.type === 'pdf' && this.pdfDoc) this.renderPage(this.currentPage);
                else if (this.currentFile && this.currentFile.type === 'image') this.$nextTick(() => this.adjustContentWrapperForImage());
            },
            toggleImageZoom() {
                this.zoomLevel = this.zoomLevel === 1.0 ? 1.8 : 1.0;
                this.$nextTick(() => this.adjustContentWrapperForImage());
            },
            onImageLoad(event) {
                const img = event.target;
                this.imageNaturalWidth = img.naturalWidth;
                this.imageNaturalHeight = img.naturalHeight;
                this.adjustContentWrapperForImage();
            },
            adjustContentWrapperForImage() {
                const imgElement = this.$refs.previewImage;
                const wrapper = this.$refs.contentWrapper;
                if (!imgElement || !wrapper) return;
                const naturalWidth = this.imageNaturalWidth || imgElement.naturalWidth || 400;
                const scaledWidth = naturalWidth * this.zoomLevel;
                wrapper.style.width = scaledWidth + 'px';
                wrapper.style.minWidth = scaledWidth + 'px';
                const imageViewDiv = imgElement.parentElement;
                if (imageViewDiv) imageViewDiv.style.width = scaledWidth + 'px';
            }
        }
    });
</script>
</body>
</html>
