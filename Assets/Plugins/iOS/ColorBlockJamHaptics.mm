// Short haptic taps for MobileHapticService (Assets/_Project/Scripts/Core/Haptics).
// style: 0 light, 1 medium, 2 heavy (UIImpactFeedbackStyle).
#import <UIKit/UIKit.h>

extern "C" void ColorBlockJamHapticImpact(int style)
{
    UIImpactFeedbackStyle impact = style == 0 ? UIImpactFeedbackStyleLight
        : style == 1 ? UIImpactFeedbackStyleMedium
        : UIImpactFeedbackStyleHeavy;
    UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:impact];
    [generator prepare];
    [generator impactOccurred];
}
