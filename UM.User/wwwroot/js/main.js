document.addEventListener('DOMContentLoaded', () => {

    // Toggle Members Sidebar
    const toggleMembersBtn = document.getElementById('toggleMembersBtn');
    const membersSidebar = document.getElementById('membersSidebar');

    if (toggleMembersBtn && membersSidebar) {
        toggleMembersBtn.addEventListener('click', () => {
            if (window.innerWidth <= 1024) {
                membersSidebar.classList.toggle('show-mobile');
            } else {
                membersSidebar.classList.toggle('hidden');
            }
        });
    }

    // Toggle Sidebar on mobile
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const sidebar = document.querySelector('.sidebar');
    if (mobileMenuBtn && sidebar) {
        mobileMenuBtn.addEventListener('click', () => {
            sidebar.classList.toggle('show-mobile');
        });
    }

    // Server Switching
    const serverItems = document.querySelectorAll('.server-item');
    const currentServerName = document.getElementById('currentServerName');
    const currentServerDesc = document.getElementById('currentServerDesc');

    serverItems.forEach(item => {
        item.addEventListener('click', function() {
            serverItems.forEach(s => s.classList.remove('active'));
            this.classList.add('active');
            
            if (currentServerName && currentServerDesc) {
                currentServerName.textContent = this.getAttribute('data-server');
                currentServerDesc.textContent = this.getAttribute('data-desc');
            }
        });
    });

    // Channel Switching
    const channelItems = document.querySelectorAll('.channel-list .channel-item');
    const chatTitle = document.getElementById('chatTitle');
    const chatDesc = document.getElementById('chatDesc');

    channelItems.forEach(item => {
        item.addEventListener('click', function() {
            channelItems.forEach(c => c.classList.remove('active'));
            this.classList.add('active');

            const channelNameEl = this.querySelector('.channel-name');
            if (channelNameEl && chatTitle) {
                const name = channelNameEl.textContent.trim();
                chatTitle.textContent = name;
                if (chatDesc) {
                    chatDesc.textContent = `Đây là channel #${name}`;
                }
            }
            
            if (window.innerWidth <= 768 && sidebar) {
                sidebar.classList.remove('show-mobile');
            }
        });
    });

    // Message Input
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const chatHistory = document.getElementById('chatHistory');

    // Auto-resize textarea
    if (messageInput) {
        messageInput.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
            if (this.value === '') {
                this.style.height = 'auto';
            }
        });

        messageInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });
    }

    if (sendBtn) {
        sendBtn.addEventListener('click', sendMessage);
    }

    function sendMessage() {
        const text = messageInput.value.trim();
        if (!text) return;

        // Replace newlines with <br>
        const formattedText = text.replace(/\n/g, '<br>');

        const time = new Date().toLocaleTimeString('en-US', {hour: '2-digit', minute:'2-digit', hour12: true});
        
        const msgHTML = `
            <div class="message my-message">
                <div class="msg-avatar">TU</div>
                <div class="msg-body">
                    <div class="msg-header">
                        <span class="msg-author">Bạn</span>
                        <span class="msg-time">${time}</span>
                    </div>
                    <div class="msg-text">${formattedText}</div>
                </div>
            </div>
        `;

        chatHistory.insertAdjacentHTML('beforeend', msgHTML);
        messageInput.value = '';
        messageInput.style.height = 'auto';
        chatHistory.scrollTop = chatHistory.scrollHeight;
    }

    // Reaction Click
    if (chatHistory) {
        chatHistory.addEventListener('click', (e) => {
            const reaction = e.target.closest('.reaction');
            if (reaction) {
                const countSpan = reaction.querySelector('.count');
                if (countSpan) {
                    let count = parseInt(countSpan.textContent);
                    if (reaction.classList.contains('reacted')) {
                        count--;
                        reaction.classList.remove('reacted');
                        if (count === 0) {
                            // If you want to hide it when count is 0, logic goes here
                            // reaction.remove();
                        }
                    } else {
                        count++;
                        reaction.classList.add('reacted');
                    }
                    countSpan.textContent = count;
                }
            }
        });
        
        // Scroll to bottom on load
        chatHistory.scrollTop = chatHistory.scrollHeight;
    }

});
