   M S B A M L   `   `   `  ����H  DEVA_Catalogue, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:EVA_CatalogueEVA_Catalogue  5   	   -clr-namespace:EVA_CatalogueEVA_Catalogue  5   	   +    %EVA_Catalogue.WindowSettingsDataBases   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1	viewmodelclr-namespace:EVA_Catalogue     5   	   _ [MaterialDesignThemes.Wpf, Version=4.9.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   'localclr-namespace:EVA_Catalogue  5   	     � Title$<  6Звгрузить/Выгрузить каталоги��5	   	   $	��800��5
   	   $	��800��5   	   $	��700��5   	   $	��350��5   	    � AllowsTransparency . 
������  � 
ResizeMode$ 	CanResize=�5   	   $��14(�5   	   $��Regular"�5   	   &   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�5   	      TextRenderingMode$
 Auto=�5   	    � UseLayoutRounding .  � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$
 None=�5   	   ��5   	   0  *MaterialDesignThemes.Wpf.NotoFontExtension   � 	Resources 5      	 �� Source�� 5   
   $	 Dictionary1.xaml?�6   %    
 � DataContext
 5      '  !EVA_Catalogue.SettingsDataBasesVM  5    
   .��5#      � .+�5$   
   ��    BorderWindow#��[  6   ._�5%      �  � RowDefinitions 5&      �� 5'      $��50 �6$   �� 5(      $��50 �6$   �� 5)      �� 5*      $��50 �6$   .+�5.      � $��0��6    � ColumnDefinitions 5/      �� 50      $
��Auto �6+   �� 51      $��* �6+   �� 52      $
��Auto �6+   .+�55      �� $��0��6    $��3��56       $��10 0 0 0q�57       $��Center=�58       $��Center=�59       $��16(�5:       
���   ���5<       �� *Title+ �� RelativeSource ��  �� AncestorType$ WindowL��� 5=      $��2��6     CloseButtonStyle#��[ 5?      ��5>      �� *Accept+5?      5@      �� 5A      #��[  5B      $��1��5C      $
��Auto��5D      $
��Auto��5E      $��5,5,5.2,4.6q�5F      
�������._�5H      ]� $��Center=�6    .+�5I      �� 
�������$+��%Список оборудования��5K       �� TextAlignment$ Center=�5L      5M      5N      � 5Q      $��2��6    5R      �� 5S      $
��0.6* �6   6.   �� 5T      $
��0.4* �6   .+�5W      �� $��Stretch=�6#   $��10q�6A   $��0��6M   $��Stretch=�6^    �� DisplayMemberPath$ producer��6z     RoundedListBoxStyle#��[ 5X   W   ��5W   �   �� *ProducerList+ �� SelectedItem 5X      �� *
SelectedBD+ �� UpdateSourceTrigger$ PropertyChanged=�5Y      � 5[      $��1��6    5\      �� 5]   '   $��50 �65   �� 5^      $��50 �6,   �� 5_      $��50 �6,   �� 5`      .+�5b      �� $��0��5c      $	��250��5d      $��30��5e      $��	10,0,10,0q�5f        GeneralButton#��[ 5h      $��Удалить��5j      ��5g      �� *Delete+5j      �� 5k      $��2��5l      $	��250��5m      $��30��5n      $��	10,0,10,0q�5o      #��[ 5q      $��Выгрузить��5s      ��5p      �� *SaveFileDialogCommand+5s      �� 5t      $��1��5u      $	��250��5v      $��30��5w      $��	10,0,10,0q�5x      #��[ 5z      $'��!Добавить/Обновить��5|      ��5y      �� *OpenFileDialogCommand+5|      5~      5      �� 5�      $��4��5�      $��0��5�      $	��130��5�      $��30��5�      $��0,0,0,0q�5�      #��[ 5�      $
��Ок��5�      ��5�      �� *Accept+5�      5�      5�      �� 5�   
   -   5         ��Name$ 	ResizeTop��5�      $��6��5�        ResizeBorders#��[ 5�      $��Stretch=�5�      $	��Top=�5�       � Cursor$ SizeNS��5�      5�      �� 5�   
   -   5        $ ResizeBottom��5�      $��6��5�      $��Stretch=�5�      $��Bottom=�5�      #��[ 5�      $ SizeNS��5�      5�      �� 5�   
   -   5        $ 
ResizeLeft��5�      $��6��5�      $
��Left=�5�      $��Stretch=�5�      #��[ 5�      $ SizeWE��5�      5�      �� 5�   
   -   5        $ ResizeRight��5�      $��6��5�      #��[ 5�      $��Right=�5�      $��Stretch=�5�      $ SizeWE��5�      5�      �� 5�   
   -   5        $ ResizeTopLeft��5�      $��6��5�      $��6��5�      $
��Left=�5�      $	��Top=�5�      #��[ 5�      $ SizeNWSE��5�      5�      �� 5�   
   -   5        $ ResizeTopRight��5�      $��6��5�      $��6��5�      $��Right=�5�      $	��Top=�5�      #��[ 5�      $ SizeNESW��5�      5�      �� 5�   
   -   5        $ ResizeBottomLeft��5�      $��6��5�      $��6��5�      $
��Left=�5�      $��Bottom=�5�      #��[ 5�      $ SizeNESW��5�      5�      �� 5�   
   -   5        $ ResizeBottomRight��5�      $��6��5�      $��6��5�      $��Right=�5�      $��Bottom=�5�      #��[ 5�      $ SizeNWSE��5�      5�      5�      5�      