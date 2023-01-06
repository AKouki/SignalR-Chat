/***
    @name:触屏事件
    @param {string} element dom元素
    {function} fn 事件触发函数
***/
function v_on(obj, ev, fn) {
    if (obj.attachEvent) {
        obj.attachEvent("on" + ev, fn);
    } else {
        obj.addEventListener(ev, fn, false);
    }
}
var touchEvent = {
    /*单次触摸事件*/
    tap: function(element, fn) {
        var startTx, startTy;
        v_on(element, 'touchstart', function(e) {
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
        }, false);

        v_on(element, 'touchend', function(e) {
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY;
            // 在部分设备上 touch 事件比较灵敏，导致按下和松开手指时的事件坐标会出现一点点变化
            if (Math.abs(startTx - endTx) < 6 && Math.abs(startTy - endTy) < 6) {
                fn();
            }
        }, false);
    },

    /*两次触摸事件*/
    doubleTap: function(element, fn) {
        var isTouchEnd = false,
            lastTime = 0,
            lastTx = null,
            lastTy = null,
            firstTouchEnd = true,
            body = document.body,
            dTapTimer, startTx, startTy, startTime;
        v_on(element, 'touchstart', function(e) {
            if (dTapTimer) {
                clearTimeout(dTapTimer);
                dTapTimer = null;
            }
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
        }, false);
        v_on(element, 'touchend', function(e) {
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY,
                now = Date.now(),
                duration = now - lastTime;
            // 首先要确保能触发单次的 tap 事件
            if (Math.abs(startTx - endTx) < 6 && Math.abs(startTx - endTx) < 6) {
                // 两次 tap 的间隔确保在 500 毫秒以内
                if (duration < 301) {
                    // 本次的 tap 位置和上一次的 tap 的位置允许一定范围内的误差
                    if (lastTx !== null &&
                        Math.abs(lastTx - endTx) < 45 &&
                        Math.abs(lastTy - endTy) < 45) {
                        firstTouchEnd = true;
                        lastTx = lastTy = null;
                        fn();
                    }
                } else {
                    lastTx = endTx;
                    lastTy = endTy;
                }
            } else {
                firstTouchEnd = true;
                lastTx = lastTy = null;
            }
            lastTime = now;
        }, false);
        // 在 iOS 的 safari 上手指敲击屏幕的速度过快，
        // 有一定的几率会导致第二次不会响应 touchstart 和 touchend 事件
        // 同时手指长时间的touch不会触发click
        if (~navigator.userAgent.toLowerCase().indexOf('iphone os')) {
            v_on(body, 'touchstart', function(e) {
                startTime = Date.now();
            }, true);
            v_on(body, 'touchend', function(e) {
                var noLongTap = Date.now() - startTime < 501;
                if (firstTouchEnd) {
                    firstTouchEnd = false;
                    if (noLongTap && e.target === element) {
                        dTapTimer = setTimeout(function() {
                            firstTouchEnd = true;
                            lastTx = lastTy = null;
                            fn();
                        }, 400);
                    }
                } else {
                    firstTouchEnd = true;
                }
            }, true);
            // iOS 上手指多次敲击屏幕时的速度过快不会触发 click 事件
            v_on(element, 'click', function(e) {
                if (dTapTimer) {
                    clearTimeout(dTapTimer);
                    dTapTimer = null;
                    firstTouchEnd = true;
                }
            }, false);
        }
    },

    /*长按事件*/
    longTap: function(element, fn) {
        var startTx, startTy, lTapTimer;
        v_on(element, 'touchstart', function(e) {
            if (lTapTimer) {
                clearTimeout(lTapTimer);
                lTapTimer = null;
            }
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
            lTapTimer = setTimeout(function() {
                fn(startTx, startTy);
            }, 1000);
            //e.preventDefault();
        }, false);
        v_on(element, 'touchmove', function(e) {
            var touches = e.touches[0],
                endTx = touches.clientX,
                endTy = touches.clientY;
            if (lTapTimer && (Math.abs(endTx - startTx) > 5 || Math.abs(endTy - startTy) > 5)) {
                clearTimeout(lTapTimer);
                lTapTimer = null;
            }
        }, false);
        v_on(element, 'touchend', function(e) {
            if (lTapTimer) {
                clearTimeout(lTapTimer);
                lTapTimer = null;
            }
        }, false);
    },

    /*滑屏事件*/
    swipe: function(element, fn) {
        var isTouchMove, startTx, startTy;
        v_on(element, 'touchstart', function(e) {
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
            isTouchMove = false;
        }, false);
        v_on(element, 'touchmove', function(e) {
            isTouchMove = true;
            e.preventDefault();
        }, false);
        v_on(element, 'touchend', function(e) {
            if (!isTouchMove) {
                return;
            }
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY,
                distanceX = startTx - endTx
            distanceY = startTy - endTy,
                isSwipe = false;
            if (Math.abs(distanceX) > 20 || Math.abs(distanceY) > 20) {
                fn(distanceX, distanceY);
            }
        }, false);
    },

    /*向上滑动事件*/
    swipeUp: function(element, fn) {
        var isTouchMove, startTx, startTy;
        v_on(element, 'touchstart', function(e) {
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
            isTouchMove = false;
        }, false);
        v_on(element, 'touchmove', function(e) {
            isTouchMove = true;
            e.preventDefault();
        }, false);
        v_on(element, 'touchend', function(e) {
            if (!isTouchMove) {
                return;
            }
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY,
                distanceX = startTx - endTx
            distanceY = startTy - endTy,
                isSwipe = false;
            if (Math.abs(distanceX) < Math.abs(distanceY)) {
                if (distanceY > 20) {
                    fn(distanceY);
                    isSwipe = true;
                }
            }
        }, false);
    },

    /*向下滑动事件*/
    swipeDown: function(element, fn) {
        var isTouchMove, startTx, startTy;
        v_on(element, 'touchstart', function(e) {
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
            isTouchMove = false;
        }, false);
        v_on(element, 'touchmove', function(e) {
            isTouchMove = true;
            //e.preventDefault();
        }, false);
        v_on(element, 'touchend', function(e) {
            if (!isTouchMove) {
                return;
            }
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY,
                distanceX = startTx - endTx
            distanceY = startTy - endTy,
                isSwipe = false;
            if (Math.abs(distanceX) < Math.abs(distanceY)) {
                if (distanceY < -20) {
                    fn(distanceY);
                    isSwipe = true;
                }
            }
        }, false);
    },

    /*向左滑动事件*/
    swipeLeft: function(element, fn) {
        var isTouchMove, startTx, startTy;
        v_on(element, 'touchstart', function(e) {
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
            isTouchMove = false;
        }, false);
        v_on(element, 'touchmove', function(e) {
            isTouchMove = true;
            e.preventDefault();
        }, false);
        v_on(element, 'touchend', function(e) {
            if (!isTouchMove) {
                return;
            }
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY,
                distanceX = startTx - endTx
            distanceY = startTy - endTy,
                isSwipe = false;
            if (Math.abs(distanceX) >= Math.abs(distanceY)) {
                if (distanceX > 20) {
                    fn(distanceX);
                    isSwipe = true;
                }
            }
        }, false);
    },

    /*向右滑动事件*/
    swipeRight: function(element, fn) {
        var isTouchMove, startTx, startTy;
        v_on(element, 'touchstart', function(e) {
            var touches = e.touches[0];
            startTx = touches.clientX;
            startTy = touches.clientY;
            isTouchMove = false;
        }, false);
        v_on(element, 'touchmove', function(e) {
            isTouchMove = true;
            e.preventDefault();
        }, false);
        v_on(element, 'touchend', function(e) {
            if (!isTouchMove) {
                return;
            }
            var touches = e.changedTouches[0],
                endTx = touches.clientX,
                endTy = touches.clientY,
                distanceX = startTx - endTx
            distanceY = startTy - endTy,
                isSwipe = false;
            if (Math.abs(distanceX) >= Math.abs(distanceY)) {
                if (distanceX < -20) {
                    fn(distanceX);
                    isSwipe = true;
                }
            }
        }, false);
    }
}

jQuery.fn.extend({
    tap: function(fn) {
        return touchEvent.tap(jQuery(this)[0], fn);
    },
    doubleTap: function(fn) {
        return touchEvent.doubleTap(jQuery(this)[0], fn);
    },
    longTap: function(fn) {
        return touchEvent.longTap(jQuery(this)[0], fn);
    },
    swipe: function(fn) {
        return touchEvent.swipe(jQuery(this)[0], fn);
    },
    swipeLeft: function(fn) {
        return touchEvent.swipeLeft(jQuery(this)[0], fn);
    },
    swipeRight: function(fn) {
        return touchEvent.swipeRight(jQuery(this)[0], fn);
    },
    swipeUp: function(fn) {
        return touchEvent.swipeUp(jQuery(this)[0], fn);
    },
    swipeDown: function(fn) {
        return touchEvent.swipeDown(jQuery(this)[0], fn);
    }
});

// Adapt to mobile devices
// 适配移动端设备
$(function () {
    function IsPC() {
        var userAgentInfo = navigator.userAgent;
        var Agents = ["Android", "iPhone", "SymbianOS", "Windows Phone", "iPad", "iPod"];
        var flag = true;
        for (var v = 0; v < Agents.length; v++) {
            if (userAgentInfo.indexOf(Agents[v]) > 0) {
                flag = false;
                break;
            }
        }
        return flag;
    }
    var appShowState = 0;
    if (!IsPC()) {
        $('.app').swipeRight((res) => {
            if (Math.abs(res) > 200) {
                if (appShowState == 0) {
                    $(".sidebar").css("display", "flex");
                    $(".users-container").hide();
                    appShowState = -1;
                } else if (appShowState == 1) {
                    $(".sidebar").hide();
                    $(".users-container").hide();
                    appShowState = 0;
                }
            }
        })
        $('.app').swipeLeft((res) => {
            if (Math.abs(res) > 200) {
                if (appShowState == 0) {
                    $(".users-container").css("display", "flex");
                    $(".sidebar").hide();
                    appShowState = 1;
                } else if (appShowState == -1) {
                    $(".sidebar").hide();
                    $(".users-container").hide();
                    appShowState = 0;
                }
            }
        })
    }
});
