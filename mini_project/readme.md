Codetesing

mini_project

project_EX01 : apple / orange training

=> 실패 / data\yolov3_testing.cfg의 class가 0~1까지만 있었으나 image labeling 할 때 class의 값을 15부터 시작한것이 원인.

==> 해결방법 : labeling을 predefined class를 지우고 다시 labeling.
