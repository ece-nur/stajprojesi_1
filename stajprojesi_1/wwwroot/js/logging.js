/**
 * Frontend Logging Service
 * Kullanıcı etkileşimlerini ve frontend olaylarını loglar
 */
class FrontendLogger {
    constructor() {
        this.sessionId = this.generateSessionId();
        this.userAgent = navigator.userAgent;
        this.startTime = Date.now();
        this.interactionCount = 0;
        
        // Sayfa yüklendiğinde log
        this.logPageLoad();
        
        // Global event listeners
        this.setupGlobalListeners();
        
        console.log('🔍 Frontend Logger başlatıldı');
    }

    /**
     * Benzersiz session ID oluşturur
     */
    generateSessionId() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    /**
     * Sayfa yükleme logu
     */
    logPageLoad() {
        const pageInfo = {
            url: window.location.href,
            title: document.title,
            referrer: document.referrer,
            loadTime: performance.now(),
            timestamp: new Date().toISOString()
        };

        this.log('Sayfa Yüklendi', pageInfo, 'page_load');
    }

    /**
     * Global event listener'ları kurar
     */
    setupGlobalListeners() {
        // Tıklama olayları
        document.addEventListener('click', (e) => {
            this.logClick(e);
        });

        // Form submit olayları
        document.addEventListener('submit', (e) => {
            this.logFormSubmit(e);
        });

        // Input değişiklik olayları
        document.addEventListener('change', (e) => {
            this.logInputChange(e);
        });

        // Sayfa değişiklik olayları
        window.addEventListener('beforeunload', () => {
            this.logPageUnload();
        });

        // AJAX olayları
        this.interceptAjax();
    }

    /**
     * Tıklama olaylarını loglar
     */
    logClick(event) {
        const target = event.target;
        const clickInfo = {
            element: target.tagName.toLowerCase(),
            id: target.id || 'N/A',
            className: target.className || 'N/A',
            text: target.textContent?.substring(0, 50) || 'N/A',
            x: event.clientX,
            y: event.clientY,
            timestamp: new Date().toISOString()
        };

        this.log('Tıklama', clickInfo, 'click');
    }

    /**
     * Form submit olaylarını loglar
     */
    logFormSubmit(event) {
        const form = event.target;
        const formInfo = {
            action: form.action || 'N/A',
            method: form.method || 'N/A',
            id: form.id || 'N/A',
            className: form.className || 'N/A',
            fieldCount: form.elements.length,
            timestamp: new Date().toISOString()
        };

        this.log('Form Gönderimi', formInfo, 'form_submit');
    }

    /**
     * Input değişiklik olaylarını loglar
     */
    logInputChange(event) {
        const target = event.target;
        if (target.tagName === 'INPUT' || target.tagName === 'SELECT' || target.tagName === 'TEXTAREA') {
            const changeInfo = {
                element: target.tagName.toLowerCase(),
                type: target.type || 'N/A',
                id: target.id || 'N/A',
                name: target.name || 'N/A',
                value: target.value?.substring(0, 100) || 'N/A',
                timestamp: new Date().toISOString()
            };

            this.log('Input Değişikliği', changeInfo, 'input_change');
        }
    }

    /**
     * Sayfa kapanma logu
     */
    logPageUnload() {
        const sessionInfo = {
            sessionDuration: Date.now() - this.startTime,
            interactionCount: this.interactionCount,
            timestamp: new Date().toISOString()
        };

        this.log('Sayfa Kapatıldı', sessionInfo, 'page_unload');
        
        // Session end log'u gönder
        this.sendLog('session_end', sessionInfo);
    }

    /**
     * AJAX isteklerini yakalar ve loglar
     */
    interceptAjax() {
        const originalFetch = window.fetch;
        const self = this;

        window.fetch = function(...args) {
            const startTime = Date.now();
            const url = args[0];
            const options = args[1] || {};

            return originalFetch.apply(this, args).then(response => {
                const duration = Date.now() - startTime;
                
                const ajaxInfo = {
                    url: url,
                    method: options.method || 'GET',
                    status: response.status,
                    statusText: response.statusText,
                    duration: duration,
                    timestamp: new Date().toISOString()
                };

                self.log('AJAX İsteği', ajaxInfo, 'ajax_request');
                return response;
            }).catch(error => {
                const duration = Date.now() - startTime;
                
                const errorInfo = {
                    url: url,
                    method: options.method || 'GET',
                    error: error.message,
                    duration: duration,
                    timestamp: new Date().toISOString()
                };

                self.log('AJAX Hatası', errorInfo, 'ajax_error');
                throw error;
            });
        };
    }

    /**
     * Özel olay logu
     */
    logEvent(eventType, details, category = 'custom') {
        const eventInfo = {
            eventType: eventType,
            details: details,
            category: category,
            timestamp: new Date().toISOString()
        };

        this.log('Özel Olay', eventInfo, category);
    }

    /**
     * Hata logu
     */
    logError(error, context = '') {
        const errorInfo = {
            message: error.message,
            stack: error.stack,
            context: context,
            timestamp: new Date().toISOString()
        };

        this.log('Hata', errorInfo, 'error');
    }

    /**
     * Ana log fonksiyonu
     */
    log(action, details, category = 'general') {
        this.interactionCount++;
        
        const logData = {
            sessionId: this.sessionId,
            action: action,
            category: category,
            details: details,
            userAgent: this.userAgent,
            url: window.location.href,
            timestamp: new Date().toISOString(),
            interactionCount: this.interactionCount
        };

        // Console'a yazdır
        console.log(`🔍 [${category.toUpperCase()}] ${action}:`, logData);
        
        // Sunucuya gönder (opsiyonel)
        this.sendLog(category, logData);
    }

    /**
     * Log'u sunucuya gönderir
     */
    async sendLog(category, logData) {
        try {
            // Sadece önemli olayları gönder (performans için)
            if (['error', 'security', 'session_end', 'form_submit'].includes(category)) {
                await fetch('/api/logging/frontend', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(logData)
                });
            }
        } catch (error) {
            console.warn('Log gönderilemedi:', error);
        }
    }

    /**
     * Manuel log gönderimi
     */
    sendManualLog(action, details, category = 'manual') {
        this.log(action, details, category);
    }
}

// Global logger instance'ı oluştur
window.frontendLogger = new FrontendLogger();

// Kullanım örnekleri:
// window.frontendLogger.logEvent('Button Click', 'Login button clicked', 'user_interaction');
// window.frontendLogger.logError(new Error('Test error'), 'Test context');
// window.frontendLogger.sendManualLog('Custom Action', 'Custom details', 'custom');

