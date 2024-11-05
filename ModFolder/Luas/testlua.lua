function playSprite(sprite, duration)
    player.Sprite:Play(sprite, false, false)
    if duration then
        wait(duration)
    end
end

function onBegin()
    disableMovement()
    local level = player.Scene;
    waitUntilOnGround()
    
    player.DummyAutoAnimate = false
    player.DummyGravity = false
    wait(1)
    playSprite("sitDown", 100)  -- 改这个时间就好了
    player.DummyGravity = true
    player.DummyAutoAnimate = true
end

function onEnd()
    enableMovement()
end