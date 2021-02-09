//
//  Plugin.m
//  Clipboard
//
//  Created by umetaman on 2020/07/19.
//  Copyright © 2020 umetaman. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Cocoa/Cocoa.h>
#import <objc/runtime.h>

static NSArray* supportClasses = nil;

@interface PasteboardHelper : NSObject
+ (NSPasteboard*)getGeneral;
+ (bool)hasNSImage;
+ (NSImage*)getNSImage;
+ (void)getNSImageSize :(NSImage*)imagePtr :(int*)width :(int*)height :(int*)bitsPerPixel;
+ (unsigned char*)getPixelData :(NSImage*) image;
@end

@implementation PasteboardHelper
+ (NSPasteboard*)getGeneral {
    return [NSPasteboard generalPasteboard];
}

+ (bool)hasNSImage {
    if(supportClasses == nil){
        supportClasses = [NSArray arrayWithObjects: [NSImage class], nil];
    }
    
    NSPasteboard* gPasteboard = [PasteboardHelper getGeneral];
    return [gPasteboard canReadObjectForClasses: supportClasses options: nil];
}

+ (NSImage*)getNSImage {
    if([PasteboardHelper hasNSImage]){
        NSPasteboard* gPasteboard = [PasteboardHelper getGeneral];
        NSArray* pasteboardObjects = [gPasteboard readObjectsForClasses: supportClasses options: nil];
        NSImage* image = pasteboardObjects[0];
        
        if(image != nil){
            return image;
        }
    }
    
    return nil;
}

+ (void)getNSImageSize:(NSImage *)imagePtr :(int *)width :(int *)height :(int *)bitsPerPixel{
    *width = 0;
    *height = 0;
    *bitsPerPixel = 0;
    
    if(imagePtr != nil){
        NSData *tiffData = [imagePtr TIFFRepresentation];
        NSBitmapImageRep *bitmap = [NSBitmapImageRep imageRepWithData: tiffData];
        
        NSInteger rowBytes = [bitmap bytesPerRow];
        NSInteger pixelBits = [bitmap bitsPerPixel];
        NSInteger planeBytes = [bitmap bytesPerPlane];
        
        *width = (int)(rowBytes * 8 / pixelBits);
        *height = (int)(planeBytes / rowBytes);
        *bitsPerPixel = (int)pixelBits;
    }
}

+ (unsigned char*) getPixelData:(NSImage *)image{
    if(image != nil){
        NSData* tiffData = [image TIFFRepresentation];
        NSBitmapImageRep *bitmap = [NSBitmapImageRep imageRepWithData: tiffData];
        
        return [bitmap bitmapData];
    }
    
    return nil;
}

@end

bool hasClipboardImage(){
    return [PasteboardHelper hasNSImage];
}

void getClipboardImageSize(int* width, int* height, int* bitsPerPixel){
    NSImage* image = [PasteboardHelper getNSImage];
    [PasteboardHelper getNSImageSize: image :width :height :bitsPerPixel];
}

bool getClipboardImage(unsigned char* buffer){
    bool successCopy = false;
    
    NSImage* image = [PasteboardHelper getNSImage];
    if(image != nil){
        int width, height, bitsPerPixel;
        getClipboardImageSize(&width, &height, &bitsPerPixel);
        
        if(bitsPerPixel == 24 || bitsPerPixel == 32){
            unsigned char* pixelData = [PasteboardHelper getPixelData: image];
            int bytesPerPixel = bitsPerPixel / 8;
            
            for(int i = 1; i < height; i++){
                void* dst = buffer + i * width * bytesPerPixel;
                const void* src = pixelData + (height - 1 - i) * (width * bytesPerPixel);
                memcpy(dst, src, width * bytesPerPixel);
            }
            
            successCopy = true;
        }
    }
    
    return successCopy;
}
