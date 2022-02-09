# How to understand primitive types are immutable
## Primitive type
Primitive types such as `int`, `long`, `bool`, `string`, etc. are basic types provided by *C#* language. We can create new *class* or *struct* based on these native types. They are the core fields that can carry information.

## Mutable type
Imagine that one man whose original name is *Bruce*, but now he wants to change his name to *Dick*. Well, after changing his name, is the man who has a new name now still the one before? Of course, the answer is **YES**. One property of his was just changed, but that doesn't mean the man was not the man. Just like we are getting older. Does that mean me of today is not that me of yesterday? *No*. What I am speaking is about the *mutable* type. Every comprehensive system is always mutable. It's changing itself apart from its nature. In programming world, one class instance can make some changes inside itself, but the pointer to that instance keeps same.

## Immutable type
As just mentioned, if some kind of changes happen and the nature of that thing changes as well, we can say a new thing appears, which means the new one is not the old, just like chick comes out from egg. Number *1* is not *2*. String "hello" is not "world". Boolean *true* is not *false*. We are not able to modify the content of value *1* because *1* per se is primitive. There is nothing inside the *1*. Even if we can change something, that will make *1* not *1* any more. The most classical is `string` type. One string value is composed of some characters. Its value like "good" is unchangeable. We are always creating new values if we want to change it on *C#* side.

### Type can be designed as *immutable*
In fact, `string` is designed to be *immutable* as well as `DateTime`, `Guid`, `Regex`, etc. We are allowed to create such immutable types so that caller has to create new instances all the time for security purpose such as thread safety, reliability or some reasons else. Any change attempt to the object will fail.

## Variable vs. data (value)
Variable is created to store data. The data can be value of *Value Type* or *Reference Type* instance. For *Value Type*, variable holds all the information of that instance. For *Reference Type*, variable just holds the reference (i.e. *pointer*). The content of variable is always changeable unless the variable is *readonly* or *constant*. So, mutable variable is different from mutable type. They are two matters.